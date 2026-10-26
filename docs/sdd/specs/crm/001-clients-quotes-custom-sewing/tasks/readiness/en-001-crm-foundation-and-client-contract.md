# EN-001 — Crm foundation and client contract

**Status:** client-slice implemented; focused backend and frontend checks verified, local migration applied; manual acceptance pending. **Enables:** SC-C01–05. **Depends on:** None.

## Paths and one responsibility

Planned roots/areas: `src/Modules/Crm/Crm.{Domain,Application,Infrastructure,Api}/Crm.<Layer>/; src/Bootstrapper/Host/; docs/sdd/contracts/crm/clients-quotes.openapi.yaml; docs/sdd/contracts/openapi.yaml`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Recheck actual target schema and current Identity role policies; select existing host wiring and migration runner. Freeze specified client scope, ID convention, paginated client/contact/merge contract and errors. Register only this implemented slice as delivery proceeds. Establish isolated test DB and existing test-project dependencies.

## Verification and evidence

Test level: Prerequisite analysis / adapter proof; no business test required for documentation-only decisions.

Metadata/schema/role/DI evidence; agreed schemas/examples; focused contract and architecture test plan. No business data change in discovery.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/en-001.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Client foundation decisions recorded; contracts and task dependencies are concrete, no invented service/DB state.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.

## Execution evidence

See [client backend evidence](../../evidence/clients-backend-2026-10-26.md) for actual commands/results and scope limits. Quote-derived sharing, issued recipient invariants and future imports are not claimed implemented by the client-only slice.
