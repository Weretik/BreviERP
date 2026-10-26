# TS-005 — Version history and lifecycle actions

**Status:** planned. **Covers:** SC-Q02, SC-Q04–05. **Depends on:** TS-004.

## Paths and one responsibility

Planned roots/areas: `Crm.Application/Crm.Application/Quotes/{RepriceQuote,CopyQuote,TransferQuote,ArchiveQuote,GetQuoteVersions,GetQuoteVersion}/; Crm.Domain/Crm.Domain/Quotes/{Ownership,Events}/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Implement explicit old/new preview confirmation, append-only versions and events, copy to new root, atomic ownership transfer and reversible archive. Keep issued snapshot even after new draft. Complete SQL-scoped search/date/state filters and actor history. Number allocation remains behind EN-005, not a hardcoded format.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Two-manager scope transfer, copy number/state isolation, archived retrieval, source-update independence and stale confirmation tests; verify no sent-event copy.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-005.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

View/text edit/copy cannot refresh prices; explicit reprice is attributable and historical amounts stay intact.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
