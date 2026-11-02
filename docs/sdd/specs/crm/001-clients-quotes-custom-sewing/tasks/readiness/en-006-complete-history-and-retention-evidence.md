# EN-006 — Complete history and retention evidence

**Status:** source inventory/freeze and owner-confirmed retention verified; available-source inventory/export proceeds under the latest owner instruction; incomplete graphs remain explicit and Identity/custodian assignment is deferred. [Source evidence](../../evidence/en-006-source-inventory-2026-11-02.md). **Enables:** SC-M01–03. **Depends on:** None.

## Paths and one responsibility

Planned roots/areas: `Restricted source manifest/blob inventory and identity-role mapping; this SDD migration.md`. Paths beginning `Crm.<Layer>/` are relative to `src/Modules/Crm/`; project source root repeats the project name. Each use case separates command/query, handler, validator, DTO and scenario-local specification files. Determine adjacent concrete filenames before edits; do not combine unrelated types into a catch-all file.

## Work

Obtain complete consistent export and real file bytes or explicit accepted archival gaps. Freeze counts/hashes/time, ownership and CLIENT role mappings, approved missing-file/read-only behavior and retention/erasure policy. Do not infer 958 clients from mixed counterparties or complete history from 153 headers.

## Verification and evidence

Test level: Prerequisite analysis / adapter proof; no business test required for documentation-only decisions.

Signed/versioned manifest and accepted gap ledger, target/source owner mapping and recovery retention decision without personal data in repo.

For behavior implementation record Red → Green → Refactor → focused Regression with actual command/test and result. Discovery/documentation-only prerequisites use source/link/diff evidence instead. Evidence is **not run** at SDD creation; add a dated `evidence/en-006.md` under the feature when executing, with inputs, changed files, contract SHA where applicable, checks, limitations and outcome.

## Checkpoint

Import scope is measurable and every gap has a disposition; incomplete history cannot be called complete.

Do not advance a dependent task on an unresolved prerequisite. See [sequence](../implementation-plan.md) and the feature's verification/decision documents before declaring this task complete.

## Owner decisions and measured gaps

2026-11-02: owner confirmed current copy only and no automatic deletion. Retention reference owner-no-auto-delete-2026-11-02 closes that decision. [Source inventory](../../evidence/en-006-source-inventory-2026-11-02.md) and [dry-run](../../evidence/ts-013-dry-run-2026-11-02.md) classify all selected records; scope/owner/missing-file acceptance remains explicit. Do not mark full archive readiness complete on checksum agreement alone.

Owner follow-up2026-11-02: a full export is required before any target transfer. The owner rejected using accepted gaps in the current copy as a migration basis. Custodian/owner assignment is deferred. The current copy remains analysis-only; no missing owner is filled automatically. No automatic deletion remains in force.

Latest owner correction2026-11-02: export and work with all currently available source data, including the approved project Google workbook. Do not wait for a fuller Calculator export; the earlier full-export requirement above is superseded. Preserve missing/truncated-source evidence without inventing rows or file bodies. Identity/custodian assignment is still deferred. Export artifacts are not target-import receipts and do not activate delivery.
