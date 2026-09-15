# PKC Handoff

Use this file when continuing PKC in another coding or independent-review thread.

## Product contract

PKC is a deterministic Product/System Knowledge Compiler. A Product Owner should be able to hand generated portable knowledge to an AI and ask practical product/system questions without the AI re-reading source code.

Architecture remains:

```text
source inputs
→ deterministic analyzers/adapters
→ evidence/facts
→ feature/workflow/business-decision candidates
→ knowledge synthesis
→ canonical model
→ portable rendering
```

Do not add direct source-to-freeform-AI generation.

## Current state

```text
V0.4.4  Loren knowledge readiness              PASS / COMPLETE
V0.4.5  Jellyfin generalization               PASS / COMPLETE
V0.4.6  business logic reconstruction         INDEPENDENT RE-REVIEW FAIL / 1 BLOCKER
V0.4.7  cross-layer PO-question readiness     LOCKED
V0.5    Azure DevOps input evidence           LOCKED
```

Reviewed production checkpoint:

```text
7f652c717c17f40f99a08b126b889a27a84c6376
fix: require complete predicate dependency proof
```

Latest independent review:

```text
docs/reviews/2026-09-15-v0.4.6-independent-rereview-5.md
verdict: FAIL / FIX REQUIRED
```

Current disposition:

```text
B6.1 PASS / keep closed
B6.2 PASS / keep closed
B6.3 PASS / keep closed
B6.4 BLOCK — projector can invalidate predicate state after direct-copy proof
```

Do not start V0.4.7 or Azure DevOps.

## Closed scope

### B6.1 — PASS

Exact predicate authority uses source path + syntax `SpanStart` in the target Roslyn project semantic model. Same-line custom and genuine LINQ calls cannot share authority.

### B6.2 — PASS

Configured-item ownership remains limited to supported direct collection-item syntax; nested property objects are not promoted.

### B6.3 — PASS

Angular result/list correlation requires module-qualified service identity from lexically active relative imports. Ambiguous or unresolved ownership is omitted.

## B6.4 — remaining blocker after independent re-review 5

The previous whole-item helper dependency blocker is closed at production SHA `7f652c717c17f40f99a08b126b889a27a84c6376`: unsupported source-parameter uses such as `helper(card)`, `card.SomeMethod()`, reference identity, custom/operator whole-item semantics, etc. fail closed.

The remaining problem is the **projector effect after copy**.

Current same-type method-group proof accepts the projection when each predicate-relevant member has a direct assignment like:

```csharp
IsPublished = card.IsPublished
```

but it does not prove that later initializer writes cannot mutate the copied member through output-object side effects.

Minimal compile-valid reproduction:

```csharp
public sealed class Card
{
    public bool IsPublished { get; set; }

    private bool _blocked;
    public bool Blocked
    {
        get => _blocked;
        set
        {
            _blocked = value;
            IsPublished = false;
        }
    }
}

public IReadOnlyList<Card> GetCards() =>
    _cards
        .Where(card => card.IsPublished)
        .Select(CloneCard)
        .ToArray();

private static Card CloneCard(Card card) => new()
{
    IsPublished = card.IsPublished,
    Blocked = card.Blocked
};
```

The source item can pass `Where`; `CloneCard` then copies `IsPublished`; the later `Blocked` setter mutates the output object's `IsPublished` to false. PKC can still treat the full `Where` predicate as preserved because the proof checks the required direct copy but ignores setter semantics of unrelated initializer targets.

Required property:

```text
complete predicate dependency proof
+ direct preservation of predicate-relevant state
+ complete proof that the supported projector cannot invalidate that state afterward
→ authoritative Where rule

otherwise
→ observed-only / no authoritative Product Owner inclusion rule
```

For V0.4.6, conservative fail-closed behavior is preferred over widening analysis. It is acceptable to reject projector shapes containing custom setters, nested/unproven initializer effects, or other writes whose effect on predicate-relevant output state is not deterministically proven safe.

Required focused regression:

```text
Where(card => card.IsPublished)
→ Select(CloneCard)
→ direct IsPublished copy
→ later custom setter mutates output.IsPublished
→ no authoritative inclusion rule
```

Keep the safe direct-member PokeTrade-style defensive-clone positive regression green.

## Exact reviewed checkpoint gates

```text
CI + full PKC tests + WorkPlay + PokeTrade   34933533049 — PASS
pinned Loren                                34933533044 — PASS
Loren-main canary                           34933533104 — PASS
pinned Jellyfin                             34933533050 — PASS
```

Exact CI evidence:

```text
PKC build:                 0 warnings / 0 errors
C# tests:                  76 / 76 PASS
frontend tests:            13 / 13 PASS
WorkPlay:                  PASS
PokeTrade:                 PASS
```

Pinned Jellyfin artifact was independently downloaded and checked during re-review 5:

```text
artifact id:                    10382099834
artifact digest:                sha256:f46bf8b748bb1e044b61c74f73139b23a12d1b2b0e37fcaf17d520dd89a43113
canonical Markdown files:       504
facts:                          43,363
analysisMode project-semantic:  43,363 / 43,363
bundle parity:                  PASS
ZIP file-set parity:            PASS
ZIP byte parity:                PASS
raw .pkc leak:                  none
src/ leak:                      none
```

## Exact next coding action

Fix only B6.4, regression-first:

```text
custom-setter regression
→ generic conservative projector-effect proof
→ focused tests
→ related tests
→ full relevant suite
→ review diff
→ one coherent implementation commit
→ one push
→ exact-head CI/PokeTrade/Loren/Loren-main/Jellyfin/parity
→ independent re-review
```

Do not mark V0.4.6 complete in the coding thread. Do not advance V0.4.7 until independent PASS.
