# TS-013 — Staging, mapping and dry-run pipeline

**Status:** offline tooling implemented and locally verified; the actual source plan remains blocked by EN-006 gates. No target writes/activation. [Focused test and operational evidence](../../evidence/ts-013-dry-run-2026-11-02.md). **Covers:** SC-M01–02. **Depends on:** TS-002, TS-007, EN-006.

## Paths and one responsibility

Planned roots/areas: `Crm.Infrastructure/Crm.Infrastructure/Imports/{Staging,Mapping,Reconciliation}/; existing Host.Seed/job integration selected during implementation; tests/IntegrationTests/Crm/Imports/`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Implement offline source ingestion/staging with checksums, role classification, exact source maps, review candidates, legacy-kind classification and graph/blob dependency checks. Add dry-run/reconciliation report and no-write default following existing jobs procedure. No OAuth/auth/finance payload import.

## Verification and evidence

Test level: Focused Domain/Application, PostgreSQL/API integration and architecture checks as required by this task; no blanket full suite.

Synthetic complete/partial/truncated fixtures; source count classification equation; repeated key/hash no-op vs changed-hash conflict; no live writes during dry run.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/ts-013.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Report exactly what will migrate, map, exclude or quarantine before any target import.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.

## Offline implementation checkpoint

[Design](../../design/p7-history-dry-run.md), [runbook](../../../../../operations/data/crm-history-dry-run.md) and [actual execution](../../evidence/ts-013-dry-run-2026-11-02.md) describe verified no-write behavior.2088 selected rows are fully classified, not imported. The source completeness/accepted gaps/Identity ownership prerequisite is not bypassed; no TS-014 behavior was implemented.

Owner follow-up2026-11-02: this verified offline tool may analyze the current copy, but target transfer requires a full export first. Accepted gaps are not an approved alternative. Identity/custodian assignment is deferred; TS-014 remains gated.

Latest owner correction2026-11-02: export and work with all currently available source data, including the approved project Google workbook. Do not wait for a fuller Calculator export; the earlier full-export requirement above is superseded. Preserve missing/truncated-source evidence without inventing rows or file bodies. Identity/custodian assignment is still deferred. Export artifacts are not target-import receipts and do not activate delivery.
