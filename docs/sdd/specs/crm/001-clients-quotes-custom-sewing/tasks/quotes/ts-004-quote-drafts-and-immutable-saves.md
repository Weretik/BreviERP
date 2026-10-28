# TS-004 — Quote drafts and immutable saves

**Status:** backend and matching frontend draft slice implemented locally; automated checks verified, manual acceptance pending. See [backend evidence](../../evidence/p2-quote-backend-2026-10-26.md) and [handoff](../../evidence/p2-handoff-2026-10-27.md). **Covers:** P2 portions of SC-Q01, SC-Q03–04, SC-Q06–07. **Depends on:** TS-002, TS-003.

## Paths and one responsibility

Planned roots/areas: `Crm.Domain/Crm.Domain/Quotes/{Versions,Items,Variants,Pricing}/; Crm.Application/Crm.Application/Quotes/{CreateQuote,SaveQuoteVersion,GetQuote,GetQuotes}/; Crm.Infrastructure/Crm.Infrastructure/Persistence/; Crm.Api/Crm.Api/Quotes/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Create typed root/version/child model and additive mapping. Save validated selection intent and server basis atomically with event/requestId outcome; preserve recipient snapshot, visibility/order and private/public distinction. Allow named unresolved drafts, but ready requires resolved client. Add one-off PPE and typed legacy read model; bounded list returns capabilities and full paged metadata.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Round-trip ordered items/variants/tiers, unchanged text-only price basis, forged input rejection, 409 race and retry, one-off kind tests, snapshot schema and API contract checks.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-004.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Reload returns exactly saved content; no auto reprice or incomplete graph; full draft slice contract handoff ready.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
