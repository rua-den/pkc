# RD8-B Repair (a) Private Re-probe — Phase 1 Workspace-Only Result

Date: 2026-09-28
Candidate: `8c4055decd56e4597b2a1aa03e24b5c1a70235d5`
Phase: clean-context Phase 1 answer supplied by the user; workspace-only answer frozen; Phase 2 source cross-check pending.

## Frozen Phase 1 answer (sanitized transcription)

This English transcription preserves the user's supplied Vietnamese answer. Workspace file paths are normalized to workspace-relative paths.

**Conclusion from the workspace, without inspecting source:** With `LostDate = 01/09` and `IsCustomer` still `true` on 24/09, whether the contact can log in to CustomerWeb is **NOT PROVEN**. The date on which access ends is also **NOT PROVEN**.

1. **Login and cutoff — NOT PROVEN.** The company update workflow records when `LostDate` is set while `IsCustomer`, but does not connect that date to login permission. The CustomerUsers login workflow describes creating/updating a login, but does not establish the authentication condition or revocation time. Evidence: `knowledge/workflows/company/updatecompany.md` — State changes; `knowledge/workflows/customerusers/createorupdateloginasync.md` — Permissions and State changes.
2. **Deactivate has alternative branches** according to `isInActiveCondition`:
   - If the condition is true, set `IsActive = false`.
   - Otherwise, set `IsFormerCustomer = true`, `IsCustomer = false`, and `IsProspect = false`.

   The “Otherwise” branch means these effects are alternatives, not concurrent effects. Evidence: `knowledge/workflows/companies/deactivateasync.md` — State changes.
3. **Applicable branch and exact cutoff — NOT PROVEN.** The workspace does not connect `LostDate = 01/09` to `isInActiveCondition` or say that `LostDate` causes CustomerWeb login to stop on a particular date.

The answer's conclusion is **NOT PROVEN** for both login eligibility on 24/09 and the exact access cutoff.

## Phase 1 calibration observation

```text
exclusive effects represented as alternatives: YES
false combined proven effect present:          NO
unsupported unconditional effect observed:    NO
access eligibility/cutoff uncertainty marked: NOT PROVEN
```

The supplied answer reports that it was produced from the generated workspace without reading source. It represents the exclusive effects as alternatives and does not infer a portal-login consequence, an applicable branch, or a cutoff date. The pinned answer is now frozen for Phase 2 comparison:

```text
clean-context Phase 1 answer received:  YES (user supplied)
workspace-only / no-source boundary:   stated by the answer
exclusive effects represented as alternatives: YES
false combined proven effect remains:  NO
unsupported unconditional effect:     NO
login eligibility / cutoff:            NOT PROVEN
source-known score:                    PENDING Phase 2
```

This records the clean-context replay requested by the runbook; the workspace answer itself is the available evidence (no tool transcript was attached). Do not close repair (a) or score RD8-B until the approved Phase 2 source cross-check establishes the known behavior and compares it with this frozen answer. This workspace describes code-observed behavior, not business-approved intent.

## Evidence boundary

This report contains only sanitized behavior-level findings from the generated workspace. The answer supplied for Phase 1 states that no source was read. No target source was opened to prepare this report. It includes no target path/name, source path/body, raw fact payload, or source cross-check result. The Phase 1 answer is frozen; source comparison and scoring remain pending.

## Phase 2 — source-enabled cross-check

Performed in the approved source-enabled local environment. No source body, file path, class name, or other target-identifying detail is reproduced below — only sanitized behavior-level conclusions and general evidence descriptions, per the no-leak rule for this target.

1. **Login eligibility on 24/09 — PROVEN.** The CustomerWeb sign-in path resolves the caller's company and grants access only when that company's "is a current customer" flag and its "is active" flag are both still on. The company record's lost-customer date field is not read anywhere in that eligibility check. So with `LostDate = 01/09` and the "is a current customer" flag still on at 24/09, the contact **can** log in on 24/09.
2. **Revocation condition — PROVEN (condition), NOT a fixed date.** Access is lost the moment either the "is a current customer" flag or the "is active" flag flips off — there is no scheduled or delayed cutoff derived from the lost-customer date. Both alternative branches of the deactivation workflow (see item 3 below) end up flipping one of those two flags, so either branch removes CustomerWeb access; they differ in *which* flag is cleared, not in *whether* access is lost. There is no evidence of a specific calendar date being computed or stored as an access-cutoff date.
3. **Branch selection is independent of LostDate — PROVEN (independence).** The deactivation workflow picks its branch using an "is inactive" condition computed from the company's own open commercial activity (open deals, active projects, active agreements/invoicing), not from the lost-customer date or the current-customer flag. So `LostDate = 01/09` together with `IsCustomer = true` does not by itself determine which deactivation branch would apply; the branch depends on unrelated activity data not addressed by the posed scenario.
4. **Mutual exclusivity — PROVEN.** The two branches are a strict if/else on the same "is inactive" condition: exactly one of them runs for a given deactivation call, confirming the Phase 1 reading that they are alternatives, not concurrent effects.

### Phase 1 vs. Phase 2 comparison

- Phase 1 (workspace-only) correctly identified the two deactivation branches as mutually exclusive alternatives and correctly declined to connect `LostDate` to `isInActiveCondition` or to a login cutoff — both of those non-connections are now source-confirmed as genuine absences, not workspace gaps.
- Phase 1 left login eligibility on 24/09 as NOT PROVEN. Source shows it actually **is provable** and resolves to "still logged in" — the generated workspace under-claimed here because it had no access-eligibility fact linking the current-customer/active flags to CustomerWeb sign-in.
- Phase 1 left "date access ends" as NOT PROVEN. Source confirms there is no fixed/derived date at all — access ends only when a flag flips, so "NOT PROVEN" for a *specific date* remains correct, but the underlying condition-based revocation rule is now known and was previously missing from the workspace's answer.

### Score

**PARTIAL.** Phase 1 was correct and safely conservative on the branch-exclusivity and non-connection findings, but under-proved the login-eligibility question that the workspace evidence should have been able to settle (an eligibility rule keyed on the current-customer/active flags exists in source and is not reflected in workspace facts). No FAIL-grade error (no false combined-effect claim, no invented cutoff date) was observed in the Phase 1 answer.

### Evidence boundary (Phase 2)

This section contains only sanitized behavior-level conclusions and generalized evidence descriptions. No source code, file path, class/method name, or other target-identifying detail from the private repository is included.
