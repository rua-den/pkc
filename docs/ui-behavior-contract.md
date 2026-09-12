# UI Behavior Knowledge Contract

PKC must compile enough UI behavior for a Product Owner or AI to answer configuration and validation questions from portable knowledge without reopening source code.

This is a **knowledge completeness contract**, not a requirement to model every visual or browser detail.

## Core questions PKC must support

When the source contains enough static evidence, portable knowledge should be able to answer questions such as:

```text
What fields does this screen/form expose?
Which values/types/options can be selected?
Which fields are required?
When does a field become conditionally required?
When is a field shown/hidden or enabled/disabled?
Which permission or state controls an action or field?
Which UI value is sent to which API/request field?
Does backend validation agree with the UI constraint?
```

Example target question:

> For a CSP service, is Microsoft Subscription Id required on the UI, under what condition, and is the same requirement enforced by the backend?

If the source contains the answer but PKC cannot preserve it in `PKC_KNOWLEDGE.md` / `knowledge/`, the code-derived knowledge foundation is incomplete.

## Canonical UI evidence direction

Framework adapters should normalize UI behavior into canonical evidence instead of leaking framework-specific syntax into knowledge synthesis.

Target evidence kinds include:

```text
ui-screen
ui-route
ui-action
ui-api-call

ui-field
ui-field-option
ui-field-validation
ui-field-visibility
ui-field-enabled-state
ui-field-binding
```

The exact evidence schema may evolve, but the meaning must remain framework-neutral.

Examples:

```text
ui-field
  field = msSubscriptionId
  screen/form = ServiceConfiguration

ui-field-validation
  field = msSubscriptionId
  rule = required
  condition = serviceType == CSP

ui-field-visibility
  field = msSubscriptionId
  condition = serviceType == CSP

ui-field-option
  field = serviceType
  value = CSP

ui-field-binding
  field = msSubscriptionId
  requestField = microsoftSubscriptionId
  endpoint = POST /api/services
```

## Required semantics

### Validation

PKC should preserve statically observable constraints such as:

- required / conditionally required;
- min/max length;
- min/max numeric values;
- patterns where safely extractable;
- custom validation calls when their exact meaning is not known, without inventing semantics;
- backend validation for the same field/value when traceable.

A frontend-only requirement and a backend-only requirement are both important observations.

### Conditional behavior

PKC should preserve statically observable conditions controlling:

- visibility;
- enabled/disabled state;
- requiredness;
- action availability;
- allowed options/types.

The condition must remain attached to the behavior instead of being flattened into an unconditional claim.

### Field/value flow

When statically traceable, PKC should connect:

```text
UI field
  → form/control/model field
  → request payload field
  → API call
  → backend request/property/validation
```

Do not invent a binding when the source does not prove one. Prefer an explicit unknown over a guessed mapping.

## UI/backend consistency

PKC should preserve enough evidence for downstream knowledge to distinguish:

```text
UI requires field + backend requires field
→ consistent observed validation

UI requires field + backend requirement not observed
→ backend requirement unknown

UI requirement not observed + backend requires field
→ possible UI/backend validation gap
```

PKC must not automatically label a difference as a bug. It should report the observed mismatch and preserve uncertainty.

## Knowledge-layer expectations

```text
feature
→ important configuration requirements and conditional rules

workflow
→ exact UI field/action behavior, request mapping, validation/failure details

evidence
→ source locations and analyzer confidence/provenance
```

Low-level template/framework syntax belongs in evidence/workflow detail. Product-facing knowledge should explain the actual configuration rule.

## Analyzer fidelity

UI behavior evidence can have different confidence levels.

For example:

- literal `required` attributes or direct `Validators.required` calls may be strong syntactic evidence;
- conditional validator mutation inferred from surrounding code may be medium confidence;
- complex custom validator meaning may remain unknown unless deterministically resolvable;
- request-field mapping must not be claimed when only names happen to resemble each other.

Fallback and caveats must remain explicit.

## V0.4 exit relevance

This contract is part of code-derived knowledge readiness before V0.5.

V0.4.x does not need to support every frontend framework or every dynamic runtime behavior. It does need to prove, on at least one supported real/form benchmark, that important statically observable validation and conditional UI behavior survive into portable knowledge.

A real benchmark should include at least:

```text
a selectable type/option
an always-required field
a conditionally-required field
a conditionally-visible or enabled field
field → request/API mapping
corresponding backend validation for at least one field
```

Blind knowledge review must be able to answer those rules from `PKC_KNOWLEDGE.md` without source access.
