# TS-012 — Pinned production package export

**Status:** backend implemented; see [unified verification](../../evidence/p6-backend-checkpoint-2026-10-30.md). **Covers:** SC-S06. **Depends on:** TS-007, TS-010.

## Paths and one responsibility

Planned roots/areas: `Crm.Application/Crm.Application/CustomSewing/Production/; Crm.Infrastructure/Crm.Infrastructure/Documents/Production/; Crm.Api/Crm.Api/Quotes/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Define versioned internal package schema, selected options, composition/operation/unit/wage basis and production-visible files. Generate private artifact/hash and event for a pinned quote version. Exclude private financial/recipient fields not required for production; no Workshop network transport/order creation.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Pinned version/file visibility, master-change independence, no-private-data projection and no external call tests.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-012.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Package can be consumed later with provenance; production integration is not falsely marked implemented.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
