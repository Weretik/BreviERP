# TS-008 — Reusable templates and model suggestions

**Status:** planned. **Covers:** SC-S04. **Depends on:** TS-006.

## Paths and one responsibility

Planned roots/areas: `Crm.Application/Crm.Application/CustomSewing/{GetSuggestions,ApplyTemplate}/; Crm.Domain/Crm.Domain/CustomSewing/Templates/; Crm.Api/Crm.Api/Templates/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Store versioned templates with intro text/selections/norms, Manager management of own reusable templates, Admin management of all; issuer/document layouts remain Admin-only. Build source-style explicit suggestions from approved unique models by type/season with sample/varying norms, no leaked supplier prices or fuzzy ambiguous refs. Template apply creates a new private draft and calculates current inputs on explicit request.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Unique model deduplication, ambiguous/missing reference exclusion, template/current-cost independence, explicit apply and private introduction projection tests.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-008.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

No stale template price becomes authoritative and no suggestions silently replace user input.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
