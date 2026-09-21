using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Knowledge;

public sealed class CrossStackFeatureCandidateBuilder
{
    private static readonly Regex TemplateParameterRegex = new(@"\$\{[^}]+\}", RegexOptions.Compiled);
    private static readonly Regex RouteParameterRegex = new(@"\{[^}/]+\}|:[A-Za-z0-9_]+", RegexOptions.Compiled);

    private static readonly HashSet<string> ScreenBehaviorKinds = new(StringComparer.Ordinal)
    {
        "ui-field",
        "ui-field-option",
        "ui-field-validation",
        "ui-field-visibility",
        "ui-field-enabled-state",
        "ui-result-binding",
        "ui-list-render"
    };

    private const string CSharpFallbackWarning =
        "Some C# evidence in this workflow was analyzed without the target project's full MSBuild reference graph. Treat semantic symbol and call resolution as lower confidence.";

    private const string FrontendFallbackWarning =
        "Some frontend evidence in this workflow comes from conservative regex/template fallback analysis. Treat exact UI structure and linkage as lower confidence than AST-backed evidence.";

    private const string TransitiveMutationCausalityWarning =
        "Some transitive helper mutations were not promoted into this workflow because PKC could not prove that the helper mutated the endpoint's affected object. The raw mutation evidence remains available in the compiled fact set.";

    public FeatureCandidateDocument Build(FactDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var baseline = new FeatureCandidateBuilder().Build(document);
        var enriched = new FeatureCandidateDocument(
            "0.4.6",
            baseline.Candidates
                .Select(candidate => Enrich(candidate, document))
                .Select(FilterUnprovenTransitiveMutations)
                .ToArray());
        return new ApiFrontendBindingCandidateEnricher().Enrich(enriched, document);
    }

    private static FeatureCandidate Enrich(FeatureCandidate candidate, FactDocument document)
    {
        candidate = AddAnalysisWarnings(FilterFlowNoise(candidate));

        var endpoint = document.Facts.FirstOrDefault(fact => fact.Id == candidate.SeedFactId);
        if (endpoint is null ||
            !endpoint.Metadata.TryGetValue("httpMethod", out var endpointMethod) ||
            !endpoint.Metadata.TryGetValue("fullRoute", out var endpointRoute))
            return candidate;

        var routeKey = NormalizeRouteKey(endpointRoute);
        var apiCalls = document.Facts.Where(fact => fact.Kind == "ui-api-call")
            .Where(fact => fact.Metadata.TryGetValue("httpMethod", out var method) && string.Equals(method, endpointMethod, StringComparison.OrdinalIgnoreCase))
            .Where(fact => fact.Metadata.TryGetValue("routeKey", out var key) && string.Equals(key, routeKey, StringComparison.Ordinal))
            .ToArray();
        if (apiCalls.Length == 0) return candidate;

        var factsById = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var facts = candidate.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var relations = candidate.Relations.ToList();
        var relationKeys = relations.Select(RelationKey).ToHashSet(StringComparer.Ordinal);

        foreach (var apiCall in apiCalls)
        {
            facts[apiCall.Id] = apiCall;
            AddRelation(new EvidenceRelation(apiCall.Id, "calls-endpoint", endpoint.Id, apiCall.Source), relations, relationKeys);
            AddBindingsForApiCall(document, apiCall, facts, relations, relationKeys);
            AddResultFlowForApiCall(document, apiCall, facts, relations, relationKeys);

            var actionRelations = document.Relations
                .Where(relation => relation.Kind == "triggers-api" && relation.Target == apiCall.Id)
                .ToArray();

            var actions = actionRelations
                .Select(relation => factsById.TryGetValue(relation.FromFactId, out var fact) ? fact : null)
                .Where(fact => fact?.Kind == "ui-action")
                .Cast<EvidenceFact>()
                .ToArray();

            if (actions.Length == 0 && !string.IsNullOrWhiteSpace(apiCall.Container))
            {
                actions = document.Facts.Where(fact => fact.Kind == "ui-action")
                    .Where(fact => fact.Metadata.TryGetValue("handler", out var handler) && string.Equals(handler, apiCall.Container, StringComparison.Ordinal))
                    .ToArray();
            }

            foreach (var action in actions)
            {
                facts[action.Id] = action;
                AddRelation(new EvidenceRelation(action.Id, "triggers-api", apiCall.Id, action.Source), relations, relationKeys);
            }

            var screens = actions
                .SelectMany(action => FindScreensForAction(document, action))
                .Concat(FindScreensForApiCall(document, apiCall))
                .Concat(FindScreensForResultFlow(document, apiCall))
                .DistinctBy(screen => screen.Id, StringComparer.Ordinal)
                .ToArray();

            foreach (var screen in screens)
            {
                facts[screen.Id] = screen;
                AddScreenBehavior(document, screen, facts, relations, relationKeys);

                foreach (var route in document.Facts.Where(fact =>
                             fact.Kind == "ui-route" &&
                             fact.Metadata.TryGetValue("component", out var component) &&
                             string.Equals(component, screen.Name, StringComparison.Ordinal)))
                {
                    facts[route.Id] = route;
                    AddRelation(new EvidenceRelation(route.Id, "renders-screen", screen.Name, route.Source), relations, relationKeys);
                }
            }
        }

        var coverage = candidate.Coverage.Append("frontend-static").Distinct(StringComparer.Ordinal).ToArray();
        var unknowns = candidate.Unknowns.Where(item => item != "frontend-ui-not-analyzed").ToArray();
        var enriched = candidate with
        {
            Coverage = coverage,
            Unknowns = unknowns,
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            Relations = relations.OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray()
        };

        return AddAnalysisWarnings(FilterFlowNoise(enriched));
    }

    private static FeatureCandidate FilterUnprovenTransitiveMutations(FeatureCandidate candidate)
    {
        var factsById = candidate.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
        var removedMutationIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var mutation in candidate.Facts.Where(fact => fact.Kind == "mutation"))
        {
            var owners = candidate.Relations
                .Where(relation => relation.Kind == "mutates" && relation.Target == mutation.Id)
                .Select(relation => factsById.TryGetValue(relation.FromFactId, out var owner) ? owner : null)
                .Where(owner => owner is not null)
                .Cast<EvidenceFact>()
                .DistinctBy(owner => owner.Id, StringComparer.Ordinal)
                .ToArray();

            if (owners.Any(owner => string.Equals(owner.Id, candidate.SeedFactId, StringComparison.Ordinal)))
            {
                continue;
            }

            if (owners.Length == 1 && IsSelfOwnedTransitiveMutation(owners[0], mutation))
            {
                continue;
            }

            removedMutationIds.Add(mutation.Id);
        }

        if (removedMutationIds.Count == 0)
        {
            return candidate;
        }

        return candidate with
        {
            Facts = candidate.Facts
                .Where(fact => !removedMutationIds.Contains(fact.Id))
                .ToArray(),
            Relations = candidate.Relations
                .Where(relation =>
                    \™[[Э™Y]]][Ы’YЛђЫЫќZ[њК™[][Ы‹‘њ›ЫQXЭY
H	‰‚€\™[[Э™Y]]][Ы’YЛђЫЫќZ[њК™[][Ы‹•\™Щ]
JB€•Р\њ^J
K€[љЫ›ЭЫњИHШ[™Y]K•[љЫ›ЭЫњВ€ђ\[™
[њЪ]]™S]]][ЫђШ]\Ш[]UШ\›љ[™КB€‘\Э[Э
Эљ[™РЫЫ\\™\‹“Ь™[[
B€•Р\њ^J
B€NВ€B‚€љ]]HЭ]XИ›ЫЫ\ФЩ[“ЭЫ™Y[њЪ]]™S]]][ЫЉ]љY[ЩQXЭЭЫ™\‹]љY[ЩQXЭ]]][ЫЉB€В€Y€

ЭЫ™\‹’Ъ[™OH›Y]Щ€	‰€ЭЫ™\‹’Ъ[™OHЫЫњЭќXЭЬ€ЉH€Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJЭЫ™\‹ђЫЫќZ[™\ЉH€[]]][Ы‹“Y]Y]K•ћQЩ][YJќ\™Щ]‹Э]\€\™Щ]
H€Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJ\™Щ]
H€[]]][Ы‹“Y]Y]K•ћQЩ][YJќ\™Щ]Ю[X›Ы‹Э]\€\™Щ]Ю[X›Ы
H€Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJ\™Щ]Ю[X›Ы
JB€В€™]\›€[ЩNВ€B‚€\€\Т[\XЪ]Ь‘^XЪ]Щ[•\™Щ]B€]\™Щ]ђЫЫќZ[њК	Л‰ЛЭљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€\™Щ]”Э\ќХЪ]
ќ\Л€‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
NВ€Y€
Z\Т[\XЪ]Ь‘^XЪ]Щ[•\™Щ]
B€В€™]\›€[ЩNВ€B‚€\€\ЭЭH\™Щ]Ю[X›Ы“\Э[™^ЩЉ	Л‰КNВ€Y€
\ЭЭH
B€В€™]\›€[ЩNВ€B‚€\€\™Щ]ЭЫ™\€H\™Щ]Ю[X›ЫЛ‹›\ЭЭNВ€™]\›€Эљ[™Л‘\]X[К\™Щ]ЭЫ™\‹ЭЫ™\‹ђЫЫќZ[™\‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
NВ€B‚€љ]]HЭ]XИ›ЪYYШЬ™Y[ђ™Z]љ[ЬЉ€XЭШЭ[Y[ќШЭ[Y[ќ€]љY[ЩQXЭШЬ™Y[‹€QXЭ[Ы\ћOЭљ[™Л]љY[ЩQXЭ€XЭЛ€PЫЫXЭ[ЫЏ]љY[ЩT™[][ЫЏ€™[][ЫњЛ€TЩ]Эљ[™П€™[][Ы’Щ^\КB€В€›Ь™XXЪ
\€™Z]љ[Ь€[€ШЭ[Y[ќ‘XЭЛ•Ъ\™JXЭO‚€ШЬ™Y[ђ™Z]љ[Ь’Ъ[™ЛђЫЫќZ[њКXЭ’Ъ[™
H	‰‚€
Эљ[™Л‘\]X[КXЭђЫЫќZ[™\‹ШЬ™Y[‹“[YKЭљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€XЭ“Y]Y]K•ћQЩ][YJЫЫ\Ы™[ќ‹Э]\€ЫЫ\Ы™[ќ
H	‰‚€Эљ[™Л‘\]X[КЫЫ\Ы™[ќШЬ™Y[‹“[YKЭљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
JJJB€В€XЭЦШ™Z]љ[Ь‹’YHH™Z]љ[ЬЋВ€Y™[][ЫЉ€™]И]љY[ЩT™[][ЫЉШЬ™Y[‹’YЫЫќZ[њЛ]ZKX™Z]љ[Ь€‹™Z]љ[Ь‹’Y™Z]љ[Ь‹”ЫЭ\ЩJK€™[][ЫњЛ€™[][Ы’Щ^\КNВ€›Ь™XXЪ
\€™[][Ы€[€ШЭ[Y[ќ”™[][ЫњЛ•Ъ\™J™[][Ы€O‚€™[][Ы‹‘њ›ЫQXЭYOH™Z]љ[Ь‹’Y	‰‚€™[][Ы‹’Ъ[™OH™™YYЛ[\ЭЉJB€В€Y™[][ЫЉ™[][Ы‹™[][ЫњЛ™[][Ы’Щ^\КNВ€\€\™Щ]HШЭ[Y[ќ‘XЭЛ‘љ\њЭЬ‘Y][
XЭO€XЭ’YOH™[][Ы‹•\™Щ]
NВ€Y€
\™Щ]\И›Эќ[
B€В€XЭЦЭ\™Щ]’YHH\™Щ]В€B€B€B€B‚€љ]]HЭ]XИ›ЪYYљ[™[™ЬС›Ьђ\PШ[
€XЭШЭ[Y[ќШЭ[Y[ќ€]љY[ЩQXЭ\PШ[€QXЭ[Ы\ћOЭљ[™Л]љY[ЩQXЭ€XЭЛ€PЫЫXЭ[ЫЏ]љY[ЩT™[][ЫЏ€™[][ЫњЛ€TЩ]Эљ[™П€™[][Ы’Щ^\КB€В€Y€
Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJ\PШ[ђЫЫќZ[™\ЉJB€В€™]\›ЋВ€B‚€›Ь™XXЪ
\€љ[™[™И[€ШЭ[Y[ќ‘XЭЛ•Ъ\™JXЭO‚€XЭ’Ъ[™OHќZKYљY[Xљ[™[™И€	‰‚€Эљ[™Л‘\]X[КXЭђЫЫќZ[™\‹\PШ[ђЫЫќZ[™\‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
JJB€В€XЭЦШљ[™[™Л’YHHљ[™[™ОВ€Y™[][ЫЉ€™]И]љY[ЩT™[][ЫЉљ[™[™Л’Yљ[™ЛX\H‹\PШ[’Yљ[™[™Л”ЫЭ\ЩJK€™[][ЫњЛ€™[][Ы’Щ^\КNВ€B€B‚€љ]]HЭ]XИ›ЪYY™\Э[›ЭС›Ьђ\PШ[
€XЭШЭ[Y[ќШЭ[Y[ќ€]љY[ЩQXЭ\PШ[€QXЭ[Ы\ћOЭљ[™Л]љY[ЩQXЭ€XЭЛ€PЫЫXЭ[ЫЏ]љY[ЩT™[][ЫЏ€™[][ЫњЛ€TЩ]Эљ[™П€™[][Ы’Щ^\КB€В€Y€
Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJ\PШ[ђЫЫќZ[™\ЉJB€В€™]\›ЋВ€B‚€›Ь™XXЪ
\€љ[™[™И[€ШЭ[Y[ќ‘XЭЛ•Ъ\™JXЭO€\Ф™\Э[љ[™[™С›Ьђ\PШ[
XЭ\PШ[
JJB€В€XЭЦШљ[™[™Л’YHHљ[™[™ОВ€Y™[][ЫЉ€™]И]љY[ЩT™[][ЫЉ\PШ[’Y™™YYЛ]ZKXљ[™[™И‹љ[™[™Л’Yљ[™[™Л”ЫЭ\ЩJK€™[][ЫњЛ€™[][Ы’Щ^\КNВ‚€›Ь™XXЪ
\€™[][Ы€[€ШЭ[Y[ќ”™[][ЫњЛ•Ъ\™J™[][Ы€O‚€™[][Ы‹‘њ›ЫQXЭYOHљ[™[™Л’Y	‰€™[][Ы‹’Ъ[™OH™™YYЛ[\ЭЉJB€В€Y™[][ЫЉ™[][Ы‹™[][ЫњЛ™[][Ы’Щ^\КNВ€\€™[™\€HШЭ[Y[ќ‘XЭЛ‘љ\њЭЬ‘Y][
XЭO€XЭ’YOH™[][Ы‹•\™Щ]
NВ€Y€
™[™\€\И›Эќ[
B€В€XЭЦЬ™[™\‹’YHH™[™\ЋВ€B€B€B€B‚€љ]]HЭ]XИ›ЫЫ\Ф™\Э[љ[™[™С›Ьђ\PШ[
]љY[ЩQXЭљ[™[™Л]љY[ЩQXЭ\PШ[
B€В€Y€
љ[™[™Л’Ъ[™OHќZK\™\Э[Xљ[™[™И€€Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJ\PШ[ђЫЫќZ[™\ЉH€Xљ[™[™Л“Y]Y]K•ћQЩ][YJ\SY]Щ‹Э]\€\SY]Щ
H€\Эљ[™Л‘\]X[К\SY]Щ\PШ[ђЫЫќZ[™\‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€Xљ[™[™Л“Y]Y]K•ћQЩ][YJњЩ\ќљXЩU\H‹Э]\€Щ\ќљXЩU\JH€Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJЩ\ќљXЩU\JH€X\PШ[“Y]Y]K•ћQЩ][YJ›ЭЫ™\ђЫ\ЬИ‹Э]\€ЭЫ™\ђЫ\ЬКH€Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJЭЫ™\ђЫ\ЬКJB€В€™]\›€[ЩNВ€B‚€™]\›€Эљ[™Л‘\]X[КЩ\ќљXЩU\KЭЫ™\ђЫ\ЬЛЭљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
NВ€B‚€љ]]HЭ]XИ™X]\™PШ[™Y]Hљ[\‘›ЭУ›Ъ\ЩJ™X]\™PШ[™Y]HШ[™Y]JB€В€\€™[][ЫњИHШ[™Y]K”™[][ЫњВ€•Ъ\™J™[][Ы€O€™[][Ы‹’Ъ[™OHљ[ќ›ЪЩ\И€R\С›ЭУ›Ъ\ЩU\™Щ]
™[][Ы‹•\™Щ]
JB€•Р\њ^J
NВ‚€™]\›€Ш[™Y]HЪ]И™[][ЫњИH™[][ЫњИNВ€B‚€љ]]HЭ]XИ›ЫЫ\С›ЭУ›Ъ\ЩU\™Щ]
Эљ[™И\™Щ]
HO‚€\™Щ]”Э\ќХЪ]
”Ю\Э[K€‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€\™Щ]”Э\ќХЪ]
њЭљ[™Л€‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€\™Щ]”Э\ќХЪ]
Ъ\‹€‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€\™Щ]”Э\ќХЪ]
›Шљ™XЭ€‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€\™Щ]”Э\ќХЪ]
“ZXЬ›ЬЫЩќђ\Ь™]ЫЬ™K’”™\Э[Л€‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€\™Щ]”Э\ќХЪ]
“ZXЬ›ЬЫЩќ‘^[њЪ[ЫњЛ€‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
NВ‚€љ]]HЭ]XИ™X]\™PШ[™Y]HY[[\Ъ\ХШ\›љ[™ЬК™X]\™PШ[™Y]HШ[™Y]JB€В€\€[љЫ›ЭЫњИHШ[™Y]K•[љЫ›ЭЫњЛ•У\Э

NВ‚€Y€
Ш[™Y]K‘XЭЛђ[ћJXЭO‚€XЭ“Y]Y]K•ћQЩ][YJ[[\Ъ\У[ЩH‹Э]\€[ЩJH	‰‚€Эљ[™Л‘\]X[К[ЩK›ЫЬЩK\›ЬЫ[‹Y[XЪИ‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
JJB€В€[љЫ›ЭЫњЛђY
ФЪ\њ[XЪХШ\›љ[™КNВ€B‚€Y€
Ш[™Y]K‘XЭЛђ[ћJXЭO‚€XЭ“Y]Y]K•ћQЩ][YJ[[\Ъ\У[ЩH‹Э]\€[ЩJH	‰‚€
[ЩKђЫЫќZ[њКњ™YЩ^Y[XЪИ‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
H€[ЩKђЫЫќZ[њКќ[\]K\™YЩ^Y[XЪИ‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
JJJB€В€[љЫ›ЭЫњЛђY
њ›Ыќ[™[XЪХШ\›љ[™КNВ€B‚€™]\›€Ш[™Y]HЪ]€В€[љЫ›ЭЫњИH[љЫ›ЭЫњЛ‘\Э[Э
Эљ[™РЫЫ\\™\‹“Ь™[[
K•Р\њ^J
B€NВ€B‚€љ]]HЭ]XИQ[ќ[Y\X›O]љY[ЩQXЭ€љ[™ШЬ™Y[њС›ЬђXЭ[ЫЉXЭШЭ[Y[ќШЭ[Y[ќ]љY[ЩQXЭXЭ[ЫЉB€В€Y€
\Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJXЭ[Ы‹ђЫЫќZ[™\ЉJB€В€\€ћS[YHHШЭ[Y[ќ‘XЭЛ•Ъ\™JXЭO‚€XЭ’Ъ[™OHќZK\ШЬ™Y[€€	‰€Эљ[™Л‘\]X[КXЭ“[YKXЭ[Ы‹ђЫЫќZ[™\‹Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
JK•Р\њ^J
NВ€Y€
ћS[YK“[™Э€
B€В€™]\›€ћS[YNВ€B€B‚€™]\›€ШЭ[Y[ќ‘XЭЛ•Ъ\™JXЭO‚€XЭ’Ъ[™OHќZK\ШЬ™Y[€€	‰€Эљ[™Л‘\]X[КXЭ”ЫЭ\ЩK”]XЭ[Ы‹”ЫЭ\ЩK”]Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
JNВ€B‚€љ]]HЭ]XИQ[ќ[Y\X›O]љY[ЩQXЭ€љ[™ШЬ™Y[њС›Ьђ\PШ[
XЭШЭ[Y[ќШЭ[Y[ќ]љY[ЩQXЭ\PШ[
HO‚€ШЭ[Y[ќ‘XЭЛ•Ъ\™JXЭO‚€XЭ’Ъ[™OHќZK\ШЬ™Y[€€	‰‚€Эљ[™Л‘\]X[КXЭ”ЫЭ\ЩK”]\PШ[”ЫЭ\ЩK”]Эљ[™РЫЫ\\љ\ЫЫ‹“Ь™[[
JNВ‚€љ]]HЭ]XИQ[ќ[Y\X›O]љY[ЩQXЭ€љ[™ШЬ™Y[њС›Ь”™\Э[›ЭКXЭШЭ[Y[ќШЭ[Y[ќ]љY[ЩQXЭ\PШ[
B€В€Y€
Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJ\PШ[ђЫЫќZ[™\ЉJB€В€™]\›€ЧNВ€B‚€\€ЫЫ\Ы™[ќИHШЭ[Y[ќ‘XЭВ€•Ъ\™JXЭO€\Ф™\Э[љ[™[™С›Ьђ\PШ[
XЭ\PШ[
H	‰€\Эљ[™Л’\Уќ[Ь•Ъ]TЬXЩJXЭђЫЫќZ[™\ЉJB€”Щ[XЭ
XЭO€XЭђЫЫќZ[™\€JB€•Т\ЪЩ]
Эљ[™РЫЫ\\™\‹“Ь™[[
NВ‚€™]\›€ШЭ[Y[ќ‘XЭЛ•Ъ\™JXЭO‚€XЭ’Ъ[™OHќZK\ШЬ™Y[€€	‰€ЫЫ\Ы™[ќЛђЫЫќZ[њКXЭ“[YJJNВ€B‚€љ]]HЭ]XИ›ЪYY™[][ЫЉ]љY[ЩT™[][Ы€™[][Ы‹PЫЫXЭ[ЫЏ]љY[ЩT™[][ЫЏ€™[][ЫњЛTЩ]Эљ[™П€Щ^\КB€В€Y€
Щ^\ЛђY
™[][Ы’Щ^J™[][ЫЉJJH™[][ЫњЛђY
™[][ЫЉNВ€B‚€љ]]HЭ]XИЭљ[™И™[][Ы’Щ^J]љY[ЩT™[][Ы€™[][ЫЉHO€	ћЬ™[][Ы‹‘њ›ЫQXЭY_Ь™[][Ы‹’Ъ[™_Ь™[][Ы‹•\™Щ]HЋВ‚€љ]]HЭ]XИЭљ[™И›Ь›X[^™T›Э]RЩ^JЭљ[™И[YJB€В€\€›Э]HH[YK”Ь]
	ПЙЛ	ИЙКVМK•љ[J
NВ€›Э]HH[\]T\[Y]\”™YЩ^”™\XЩJ›Э]KћЬ\[_HЉNВ€›Э]HH›Э]T\[Y]\”™YЩ^”™\XЩJ›Э]KћЬ\[_HЉNВ€Y€
\›Э]K”Э\ќХЪ]
	ЛЙКJH›Э]HH‹И€
И›Э]NВ€™]\›€›Э]K•љ[Q[™
	ЛЙКK•УЭЩ\’[ќ\љX[ќ

NВ€BџB