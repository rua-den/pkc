# RD8 Runtime/Plugin Target Applicability Correction

Date: 2026-09-24
Checkpoint: V0.4.7-E0 / RD8 Phase C
Decision: **KNOWN APPLICABLE / BLOCKED — the approved large-repository class contains runtime plugins, but current PKC evidence did not recover them**

## Why this review exists

The previous RD8 runtime/plugin audit correctly established that the available Jellyfin and Loren external artifacts do not exercise `runtime-plugin-load` and therefore cannot close RD8-C.

At that point the private-target disposition was still written as either:

1. accepted `N/A` if the target had no applicable runtime/plugin topology; or
2. validation on another approved real repository/corpus.

A previously saved sanitized reconnaissance of the intended large mixed repository has now been recovered and changes that premise. The target class **does contain** runtime-loaded plugin modules.

This is a state correction only. It does not change production code, weaken RD4 authority, or mark RD8/E0 PASS.

## Sanitized target evidence

The saved reconnaissance contains no proprietary identities: application/module names were replaced with stable aliases and counts are approximate.

It establishes all of the following:

- the repository has compile-time feature modules and a separate runtime-plugin generation;
- the runtime plugin modules are loaded by reflection from an output subfolder;
- they are copied into that runtime location by a custom post-build step;
- the host does **not** project-reference those plugin modules;
- the plugin modules are therefore reachable only through the runtime loader + build-delivery path;
- the recommended topology model explicitly called for using host loader evidence together with solution build-dependency declarations and post-build copy targets.

Sanitized shape:

```text
WEB_API_A
  -> compile-time modules by normal references
  -> [runtime reflection load; no project reference]
       SHARED_MODULE_P1
       SHARED_MODULE_P2

plugin delivery:
  solution/build dependency evidence
  + custom post-build copy into host runtime output
```

The same reconnaissance identifies missing runtime edges as a concrete PKC risk: a reference-only graph would falsely classify these modules as unreachable/unused.

## Reinterpretation of the earlier private run

Earlier RD8 notes said the first private repository “did not exercise” runtime-plugin cases. That wording described **PKC's generated discovery output**, not the repository's actual architecture.

The corrected interpretation is:

```text
repository applicability: YES
PKC runtime-plugin yield on the observed private run: missing / not proven
```

Therefore absence of a `runtime-plugin-load` edge in the previous generated profile is not evidence that the target lacks the topology.

## Relationship to RD4

RD4 remains PASS for its accepted deterministic/synthetic boundary.

Current RD4 authority intentionally requires deterministic provenance before promoting a runtime edge, including loader identity and delivery evidence. Unsupported shapes remain unresolved/UNKNOWN.

The RD4 evidence already documents gaps that may be relevant to the intended target, including:

- folder enumeration plus dynamic DLL loading when no literal assembly identity is available;
- post-build `xcopy` / `copy` command forms;
- configuration-driven plugin lists;
- loader logic outside the directly probed host source.

This review does **not** assume which one matches the private target. The sanitized reconnaissance proves the architectural shape but does not expose enough exact loader/copy syntax to choose a generic implementation safely.

## Why no production repair is made yet

A production repair would be speculative without the exact deterministic defect shape.

Required source-enabled evidence is narrow:

1. identify the loader syntax family used by the approved target;
2. identify the build/copy syntax family that proves plugin delivery;
3. identify whether solution-level project/build dependencies are needed to connect the plugin project to the host;
4. sanitize those syntax shapes without publishing proprietary names, paths, values, or source bodies.

Then use the normal regression-first loop:

```text
sanitized exact defect shape
-> focused synthetic regression that is red on current main
-> minimum generic deterministic repair
-> negative regressions preserving fail-closed behavior
-> focused / related / full relevant local verification
-> one coherent implementation commit + one push
-> rerun only affected RD8-C discovery evidence
```

Do not create an authoritative edge from name similarity, directory proximity, output-folder coincidence, or a loader without delivery/identity provenance.

## RD8-C decision

The `N/A` path is no longer valid for the intended private target class.

Current state:

```text
RD8-C applicability                         YES / PROVEN BY SANITIZED RECONNAISSANCE
RD8-C current PKC real-target proof         FAILING / MISSING YIELD
RD8-C exact implementation defect shape     EXTERNAL SOURCE-ENABLED INSPECTION REQUIRED
RD8-C acceptance                            OPEN / BLOCKED
```

Existing public external trials remain useful negative evidence only:

- Jellyfin: no applicable runtime-plugin edge in its pinned profile;
- Loren: no applicable runtime-plugin edge in its pinned profile;
- PokeTrade: PKC-owned sample, not independent real-repository applicability proof;
- current manual public benchmark corpus has no known applicable loader topology worth spending a runner merely to reproduce absence.

## Effect on RD8

```text
RD8-A fresh current-main private run        EXTERNAL / REQUIRED
RD8-B targeted Level-1 product benchmark    EXTERNAL / REQUIRED
RD8-C runtime/plugin target applicability   KNOWN BLOCKER / EXTERNAL EXACT-SHAPE INSPECTION REQUIRED
RD8                                        ACTIVE / NOT PASS
E0                                         ACTIVE / NOT PASS
E1                                         LOCKED
```

The next safe action for Phase C is **not** another broad benchmark and **not** speculative parsing. It is a bounded source-enabled inspection of the actual loader + delivery syntax, followed by a regression-first generic repair if current main cannot represent that deterministic provenance.