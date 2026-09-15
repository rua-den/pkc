# PKC Status

Last updated: 2026-09-15

## Current milestone state

```text
V0.4.4 Loren knowledge readiness                 PASS / COMPLETE
V0.4.5 real-repository generalization            PASS / COMPLETE
V0.4.6 business logic reconstruction             INDEPENDENT RE-REVIEW FAIL / 1 BLOCKER
V0.4.7 cross-layer PO-question readiness         LOCKED
V0.5 Azure DevOps input evidence                 LOCKED
```

V0.4.6 is **not complete**. Independent re-review 5 found one remaining B6.4 false-authority path in the same-type `Select` projector proof.

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

V0.4.3 remains the last accepted tool package:

```text
RuaDen.Pkc.Tool 0.4.3-preview.2
```

## Product contract governing V0.4.x

PKC must generate portable knowledge rich enough for an AI to answer practical Product Owner questions about observable behavior, business conditions and cross-layer outcomes without re-reading source code.

No deterministic proof means no authoritative product claim.

## V0.4.6 disposition

### B6.1 — PASS / keep closed

Exact C# predicate authority is re-resolved from exact invocation syntax identity. Same-line custom and genuine LINQ invocations cannot borrow authority.

### B6.2 — PASS / keep closed

Configured-item ownership remains conservative; nested property object initializers are not promoted as direct collection items.

### B6.3 — PASS / keep closed

Angular service ownership remains module-qualified and requires lexically active relative import evidence. Ambiguity/unresolved ownership is omitted rather than guessed.

### B6.4 — BLOCK

Checkpoint `7f652c717c17f40f99a08b126b889a27a84c6376` correctly closed the previous whole-item helper dependency gap. Every semantic reference to the predicate source parameter must now be a supported direct field/property read.

Independent re-review 5 found a different projector-effect gap: after proving a predicate-relevant member is directly copied, the proof does not prove that later object-initializer writes cannot mutate that member through a custom setter.

Compile-valid counterexample:

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

The source card can pass `Where` with `IsPublished == true`; the clone copies `IsPublished = true`; then the `Blocked` setter changes the returned clone to `IsPublished == false`. Current proof can still promote the `Where` predicate because it only requires that the direct `IsPublished` copy exists and does not validate the side effects of the later `Blocked` initializer write.

Required generic boundary:

```text
predicate dependencies completely proven
+ required predicate state copied
+ entire supported projector proven unable to invalidate that state
→ authoritative product-level inclusion rule

otherwise
→ observed-only / omitted authoritative rule
```

Conservative rejection of custom setters / unproven initializer effects is acceptable in V0.4.6.

## Exact automation for reviewed checkpoint

All exact-SHA gates for `7f652c717c17f40f99a08b126b889a27a84c6376` are green:

```text
CI + PKC tests + WorkPlay + PokeTrade   34933533049 — PASS
pinned Loren                            34933533044 — PASS
Loren-main canary                       34933533104 — PASS
pinned Jellyfin                         34933533050 — PASS
```

Independent CI verification:

```text
PKC build:       0 warnings / 0 errors
C# tests:        76 / 76 PASS
frontend tests:  13 / 13 PASS
WorkPlay:        PASS
PokeTrade:       PASS
```

Pinned Jellyfin artifact independently downloaded during re-review 5:

```text
artifact id:                    10382099834
artifact digest:                sha256:f46bf8b748bb1e044b61c74f73139b23a12d1b2b0e37fcaf17d520dd89a43113
canonical Markdown files:       504
facts:                          43,363
analysisMode project-semantic:  43,363 / 43,363
bundle canonical parity:        PASS
portable ZIP file-set parity:   PASS
portable ZIP byte parity:       PASS
raw .pkc leak:                  none
src/ source-tree leak:          none
```

## Exact next action

Stay in V0.4.6. Fix only the B6.4 projector-effect blocker regression-first:

```text
focused custom-setter regression
→ minimum generic fail-closed projector-effect fix
→ focused/related/full validation
→ one coherent implementation commit/push
→ exact-head gates
→ new independent V0.4.6 re-review
```

Do not start V0.4.7. Do not start Azure DevOps ingestion.
