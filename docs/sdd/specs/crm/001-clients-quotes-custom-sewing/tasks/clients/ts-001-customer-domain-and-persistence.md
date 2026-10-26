# TS-001 — Customer domain and persistence

**Status:** client-slice backend implemented; focused checks verified, frontend handoff pending. **Covers:** SC-C01–05. **Depends on:** EN-001.

## Paths and one responsibility

Planned roots/areas: `Crm.Domain/Crm.Domain/Customers/, Contacts/; Crm.Infrastructure/Crm.Infrastructure/Persistence/; tests/UnitTests/Crm/Customers/; tests/IntegrationTests/Crm/Customers/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Add typed customer/contact IDs and invariants, owner/active/revision and external identities. Map additive crm schema/CrmDbContext through existing module-schema policy; generate reviewed migration, no edit to applied migrations. Prove exact-ID uniqueness, primary-contact race and merge atomicity without customer name/email global uniqueness.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Domain Red/Green, PostgreSQL mapping/constraint/concurrency tests on empty and pre-existing-module test databases; migration up/forward recovery rehearsal; update database map.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-001.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Existing Catalog/Reference data unchanged; no duplicate primary contact or source identity; no primitive Domain FK.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.

## Execution evidence

See [client backend evidence](../../evidence/clients-backend-2026-10-26.md) for actual commands/results and scope limits. Quote-derived sharing, issued recipient invariants and future imports are not claimed implemented by the client-only slice.
