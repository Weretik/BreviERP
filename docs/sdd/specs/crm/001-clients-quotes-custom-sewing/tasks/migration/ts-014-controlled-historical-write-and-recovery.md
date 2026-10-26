# TS-014 — Controlled historical write and recovery

**Status:** planned. **Covers:** SC-M01–03. **Depends on:** TS-013, TS-005, TS-010, TS-011.

## Paths and one responsibility

Planned roots/areas: `Crm.Infrastructure/Crm.Infrastructure/Imports/; approved job orchestration/configuration; migration evidence report`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Execute run-owned bounded graph imports under the existing approved environment/jobs procedure, preserve source versions/amounts/events, keep deliveries inert, reconcile and rebuild search. Rehearse rollback before activation using coordinated DB/private-store backup. Activate only accepted complete/read-only scope.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Dry-run→write→repeat→reconcile→restore report with actual source/target counts, hashes/FKs and sampled historical PDF/visibility evidence. Never fabricate missing rows.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-014.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Every source row and file has classified outcome; no source identity duplicate, half quote, automatic email or loss of existing master data.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.
