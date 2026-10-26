# TS-009 — Versioned issuer and template settings

**Status:** planned. **Covers:** SC-D01. **Depends on:** EN-003, EN-005.

## Paths and one responsibility

Planned roots/areas: `Crm.Domain/Crm.Domain/Documents/{Issuer,Templates}/; Crm.Application/Crm.Application/Documents/ReviseIssuer/; Crm.Infrastructure/Crm.Infrastructure/Persistence/; Crm.Api/Crm.Api/Documents/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Implement Admin-only immutable issuer/template versions and active pointer, validated legal/contact/media fields, verified tax text and concurrent number allocator under confirmed policy. Do not mix client banking and seller settings or edit previously pinned settings.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Issuer validation/access, active-pointer conflict, absent legal values, number allocation race and historical settings preservation tests.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-009.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Verified active issuer settings can be pinned; no old quote legal details drift.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
