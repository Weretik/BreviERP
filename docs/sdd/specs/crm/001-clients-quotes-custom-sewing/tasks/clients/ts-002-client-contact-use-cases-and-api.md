# TS-002 — Client/contact use cases and API

**Status:** client-slice implemented; focused backend and frontend checks verified, local migration applied; manual acceptance pending. **Covers:** SC-C01–05. **Depends on:** TS-001.

## Paths and one responsibility

Planned roots/areas: `Crm.Application/Crm.Application/Customers/{CreateCustomer,GetCustomers,GetCustomer,ReviseCustomer,MergeCustomers}/; Crm.Api/Crm.Api/Customers/; tests/IntegrationTests/Crm/Api/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Implement thin transport, one CQRS handler/type per file, scenario-local SQL projections/specifications, bounded filters/page counts. Implement contact create/revise under root revision and Admin merge with both revisions. Enforce scope and requestId at server boundary. Publish actual client operations and clean contract handoff.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Focused use-case/API tests for create/read/update/merge, deactivation, duplicate source keys, privacy, pagination and concurrent saves; contract wire examples.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-002.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Existing Clients menu can consume verified client APIs at a pinned clean revision; no quote API falsely declared ready.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.

## Execution evidence

See [client backend evidence](../../evidence/clients-backend-2026-10-26.md) for actual commands/results and scope limits. Quote-derived sharing, issued recipient invariants and future imports are not claimed implemented by the client-only slice.
