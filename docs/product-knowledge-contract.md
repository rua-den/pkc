# Product Knowledge Contract

PKC compiles implementation evidence into product/system knowledge for Product Owners and AI assistants. This contract defines the knowledge classes that must remain available even when they are not authoritative business intent.

## Knowledge classes

PKC must keep these concepts distinct:

1. **Business conditions** — when an action, item, state or outcome is allowed, visible, eligible, blocked or selected.
2. **Value lineage / provenance** — where a value came from, including copies, inheritance-like propagation, derivations and overrides across entities or layers.
3. **Mutation / causality** — what observed code path can change a value or state and what downstream state it changes.

These classes may overlap, but one must not be silently promoted into another.

## Observed behavior is not automatically business intent

Implementation evidence can be useful and important without proving approved product intent.

For example:

```csharp
set
{
    _blocked = value;
    IsPublished = false;
}
```

PKC may record the observed causal fact:

```text
writing Blocked can change IsPublished to false
```

It must not automatically rewrite that as an approved product rule such as:

```text
business requires Blocked => IsPublished == false
```

unless a separate authoritative input proves that intent.

Failing to promote a fact into a business rule must not delete the underlying causal evidence.

## Value lineage is first-class product knowledge

A Product Owner must be able to ask where a value came from even when the UI did not directly provide it.

Example model:

```text
ProductGroup.A
    ↓ copied/inherited when Product is created or updated
Product.A
    ↓ copied/inherited when Service is created or updated
Service.A
```

A supported answer should be able to explain, where source evidence proves it:

```text
Service.A is populated from Product.A.
Product.A is populated from ProductGroup.A.
Observed lineage: ProductGroup.A → Product.A → Service.A.
```

PKC should preserve enough evidence to distinguish:

- snapshot/copy semantics from dynamic/reference-derived values;
- initial value source from later overrides;
- direct assignment from computed derivation;
- backend-derived values from UI-supplied values;
- the path that can later mutate the value.

Representative PO questions include:

```text
Where does Service.A come from besides the UI?
Why is this status/value different from the selected Product now?
If ProductGroup.A changes, do existing Product or Service records change automatically?
What code path can change this field after creation?
What was the last observed source/derivation step before this value was persisted or returned?
```

## Authority and retention rule

The authority boundary is:

```text
deterministically proven observable behavior
→ may become an authoritative product-behavior claim

observed implementation evidence without full product authority
→ retain as lower-authority lineage / mutation / causal evidence

unsupported inference
→ do not invent
```

Conservative downgrade is preferred over a false Product Owner claim. Conservative downgrade must not erase useful implementation evidence that can answer diagnostic or provenance questions.

## Missing evidence-source rendering

Only integrated evidence sources should generate detailed knowledge sections.

Until Azure DevOps is integrated, PKC must not spam every feature/workflow Markdown file with repeated placeholders such as:

```text
Azure DevOps history: unknown
Product intent: unavailable from Azure DevOps
```

Instead, unavailable evidence sources should be declared once at the global knowledge boundary/index level.

A feature/workflow should mention an unknown only when that unknown materially affects the answer for that feature, such as an unresolved runtime feature flag, database value, external response or configuration value.

This keeps the portable knowledge pack high-signal while preserving honest uncertainty.

## Architectural consequence

PKC is not only a rule extractor. The evidence model and synthesis pipeline must eventually support a graph that can connect:

```text
value origin
→ copy / derivation / override
→ persisted or returned value
→ later mutation causes
→ observable outcome
```

The implementation can evolve incrementally, but future milestone work must not optimize business-rule promotion in a way that discards these lineage and causality paths.
