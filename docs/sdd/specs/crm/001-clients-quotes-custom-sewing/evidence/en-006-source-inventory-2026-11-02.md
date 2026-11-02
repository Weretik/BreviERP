# EN-006 current archive assessment — 2026-11-02

Owner-authorized scope: assess the current Calculator copy and implement offline dry-run. Owner confirmed on2026-11-02 that only the current copy is available; retention is **no automatic deletion, deletion only by separate decision**. Decision reference: owner-no-auto-delete-2026-11-02. This is not an instruction to import, repair damaged history, invent missing files or silently assign users.

## Measured source

The restricted source inventory and reproducible scanner are stored outside this repository under the local P7-source working area. No personal data/raw source rows are copied into this document or test fixtures. The source is the Calculator D1 export collected2026-10-22, source schema v211; the workbook remains the field/pricing reference, not a substitute for missing history.

All7 original artifact SHA256 checks matched. Native manifest describes47tables,42included,18699exported rows; actual included array lengths match all manifest counts.33tables are all_available_rows_read,9partial,5excluded auth/connection. Full-backup and transaction-consistency flags are false. Hash/count agreement does not establish completeness.

| Area | Measured evidence | Disposition |
| --- | --- | --- |
| Counterparties |958total:870CLIENT,43supplier-only,45finance-only | Only explicit CLIENT membership can be a customer candidate; no supplier/finance duplication |
| Quotes |153headers;122without exported version;123without declared current version;109without counterparty link | Missing graphs/recipients remain review-only |
| JSON snapshots |143item snapshots,31version snapshots,5changes,91before,140after and9custom payloads invalid;419cells contain detailed bracketed truncation markers | Quarantine; never trim markers or synthesize values |
| Dependencies |140items,504changeevents and81send-history rows reference absent versions;17overrides reference absent items | Account explicitly and block affected graphs |
| Attachments |30metadata rows,698495declared bytes; no exported R2 bodies/original content hashes | Metadata alone never means available private bytes |
| Owners |3distinct source actor emails; no approved target Identity/custodian mapping supplied | Use opaque owner keys; no automatic account/user assignment |

## Readiness boundary and decisions

The source inventory/freeze, exact omissions and owner-approved retention decision are established. EN-006 **is not a full archive-readiness completion**: a complete consistent export or accepted restricted gap scope, ownership/custodian mapping and missing-file/read-only acceptance remain explicit gates before TS-014. The owner stating that no other export exists is not treated as approving every archival gap or invented recovery.

TS-013's offline adapter proof can inspect this incomplete source and return a blocked plan without reading/writing the Brevi database. Frozen controls record database/manifest/options hashes and the control-file hash; reports keep safe counts, opaque IDs/hashes and fixed reason codes. Native files, actual actor values and protected reports stay outside Git. Retention reference closes only the retention decision; source/gap and owner approvals are not fabricated.

Verification: independent source scanner compares all supplied checksum entries, native counts, explicit roles, referential IDs, JSON validity and metadata-only blob inventory. Current normalized-reader/planner/Host operational results are recorded separately after their focused tests and actual no-write run. No live Google Sheet refresh, migration, target import, mail, account creation or deletion is claimed by this source audit.

Owner follow-up2026-11-02: accepted restricted-gap migration was rejected; a full export is required before transfer. Custodian assignment is deferred. These decisions supersede the earlier open accepted-gap option; inventory findings remain unchanged.
