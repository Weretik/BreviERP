# TS-010 — Ready validation and PDF artifacts

**Status:** local backend implementation verified 2026-10-29, with hosted worker verification recorded separately; frontend/deployment acceptance remains open. **Covers:** SC-D01–02, SC-D04, SC-Q07. **Depends on:** TS-005, TS-007, TS-009. [Execution evidence](../../evidence/ts-010-2026-10-29.md).

## Paths and one responsibility

Planned roots/areas: `Crm.Application/Crm.Application/Quotes/PrepareQuote/; Crm.Application/Crm.Application/Documents/{RenderQuotePdf,GetQuoteArtifact}/; Crm.Infrastructure/Crm.Infrastructure/Documents/; Crm.Api/Crm.Api/Documents/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Validate ready input/recipient/issuer/tax/visible tiers; pin immutable issued version and number policy. Build explicit public projection, render versioned PDF and store private artifact/hash/status. Support async generation status/retry/download from saved version only; render cannot mark sent.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Ready blockers and version pin tests, artifact idempotency, escaped payloads, multipage rendered text/page inspection with private sentinel fields absent, historical download checksum. The local implementation separates ready, artifact state/API and projection-to-renderer tests from the hosted worker loop; do not infer a hosted-run pass from the other checks.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. The dated [TS-010 execution evidence](../../evidence/ts-010-2026-10-29.md) records inputs, changed boundaries, checks, limitations and outcome; this task file does not substitute for execution results.

## Checkpoint

Customer receives exactly selected public version with no internal cost/hidden option/private file exposure.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.

## Implemented local boundaries

`Quote.PrepareReady` and immutable `QuoteIssue` pin quote version, seller version, template version, number and UTC ready time. `PrepareQuote` validates recipient, included/visible content, supported UAH/tax basis, positive visible tier prices, pricing blockers, active settings and provisioned logo. It allocates the number and writes the issue in the same quote mutation transaction. `Documents/Projection` constructs an allowlisted customer projection from those pinned rows. `RenderQuotePdf` queues a versioned private artifact; `GetQuoteArtifact` reads metadata or saved PDF bytes after quote-scope authorization. Artifact state/byte storage and document source are separate Infrastructure responsibilities. HTTP ready and artifact operations are defined by canonical `crm/quote-drafts.openapi.yaml` and `crm/quote-pdf.openapi.yaml`.

Two additive Crm migrations, `20261029135919_AddQuoteIssues` and `20261029140410_AddQuoteArtifactIssuePin`, were applied only to local `brevidb` after a CRM-only backup. The frontend still consumes the older immutable P4 backend SHA; no FE-008 ready/PDF consumer handoff is claimed.
