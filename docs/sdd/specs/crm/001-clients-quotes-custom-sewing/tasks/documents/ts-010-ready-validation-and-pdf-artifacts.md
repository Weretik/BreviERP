# TS-010 — Ready validation and PDF artifacts

**Status:** planned. **Covers:** SC-D01–02, SC-D04, SC-Q07. **Depends on:** TS-005, TS-007, TS-009.

## Paths and one responsibility

Planned roots/areas: `Crm.Application/Crm.Application/Quotes/PrepareQuote/; Crm.Application/Crm.Application/Documents/{RenderQuotePdf,GetQuoteArtifact}/; Crm.Infrastructure/Crm.Infrastructure/Documents/; Crm.Api/Crm.Api/Documents/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Validate ready input/recipient/issuer/tax/visible tiers; pin immutable issued version and number policy. Build explicit public projection, render versioned PDF and store private artifact/hash/status. Support async generation status/retry/download from saved version only; render cannot mark sent.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Ready blockers and version pin tests, artifact idempotency, escaped payloads, multipage rendered text/page inspection with private sentinel fields absent, historical download checksum.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-010.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Customer receives exactly selected public version with no internal cost/hidden option/private file exposure.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
