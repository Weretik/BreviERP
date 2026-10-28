# Historical transfer, rollout and rollback

The workbook's five quote tabs are header-only; do not schedule an import of nonexistent rows. The partial database artifact contains useful evidence but is not a complete historical backup. This migration is separate from the already implemented Catalog import. No import, schema migration or job runs while writing this specification.

## Source freeze and mapping

1. EN-006 records source version/extraction interval, consistency/completeness flags, per-table row counts, file checksums and actual blob inventory. Obtain a complete source graph or a written accepted-gap scope; do not silently mark partial history complete.
2. Use restricted staging outside live Crm tables. Record source dataset/table/key, raw payload hash, import run and dependency status. Source payloads may contain personal data: no repository fixtures or verbose logs. Any malformed/truncated payload is quarantined with a reason.
3. Classify counterparties by explicit role. Only CLIENT rows map to Customer; preserve source IDs, aliases and external CRM identities. Finance-only/supplier-only records stay out of the client master; dual roles get an explicit association, not cloned Reference suppliers. Match exact external IDs first, then stage tax/name/channel candidates for review; never fuzzy-merge automatically.
4. Map source manager emails to approved existing Identity user IDs through a reviewed mapping. Missing/deactivated users stage as unresolved; Admin may explicitly assign a target custodian with source author preserved in historical audit. Never create logins or import tokens to “fix” it.
5. Include owned introduction selection templates in source staging/mapping; preserve slot/layout refs and confirm exact featured Article mappings without name guessing.
6. Classify legacy `individual` payloads into supported typed private configuration or read-only legacy content, with raw hash and missing-data flags. Unknown kinds do not become one-off PPE or Products by default.

## Ordered execution

| Step | Transfer                                                             | Reconciliation and stop condition                                                                                                               |
| ---- | -------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| M0   | Backup target database and private storage; dry-run manifest         | Verify target/environment, restore procedure, schema version, candidate counts, access and mapping gates                                        |
| M1   | Role-filtered customers/contacts/source identities                   | Every input has imported/already-mapped/needs-review/excluded status; duplicate source keys stop affected batch                                 |
| M2   | Private file bytes + custom versions/dependencies                    | Hash/size/MIME/owner/visibility match; missing byte state explicit; no usable-file claim from metadata                                          |
| M3   | Quote roots, versions, items, variants, calculation lines, overrides | Exact parent/version graph, source number collision report, ordered children and original monetary values; missing mandatory graphs quarantined |
| M4   | Transfer/change events, inert email history and document artifacts   | Timestamps/actors/source hashes retained; no pending send and no current-master recalculation                                                   |
| M5   | Rebuild scoped search projection                                     | Search/count respects access; source search index is not copied as history                                                                      |
| M6   | Reconciliation report and controlled activation                      | All row/file counts classified, no unresolved required FK, sampled public PDF/content parity, restore rehearsal and accepted gap ledger         |

Process bounded transactions per graph, not an all-tables transaction or unbounded load. SourceMappings are unique `(sourceDataset,sourceEntity,sourceId)`; same source key/hash replays as a no-op. Changed source payload is a review conflict, not overwrite of an issued target version. Failed batch does not commit a half quote. Record per-run created IDs for a reversible pre-activation import. A quote imported with incomplete parts is visible only as explicitly read-only/incomplete history if the accepted gap policy permits; it cannot be treated as newly ready or sent.

## Dry run, reports and acceptance

For each dataset: source manifest count = staged count; staged = imported + alreadyMapped + needsReview + excluded + invalid, with disjoint row statuses. Per source quote compare version count, item/variant order/count, tier amounts/currency/visibility, recipient, owner mapping, archive flags, and file-byte hashes. “All available rows read” is not equivalent to transaction consistency. Expected counts must come from the frozen complete/accepted-scope manifest, not hardcoded partial numbers in this SDD.

SC-M01: given staged mixed-role counterparties and exact mappings, when dry-run/import repeats, then only clients are created once, no supplier-only duplicates, changes conflict visibly. SC-M02: given missing quote children/blob bytes, when reconciling, then records are quarantined or explicitly incomplete, with no fabricated prices/files/sends. SC-M03: given a successful import batch and backup, when rollback rehearses, then target-owned existing Catalog/Reference/client changes and prior issued history survive, restored bytes/hash/counts match.

## Rollout

P3 adds `20261028000852_AddQuoteLifecycle` (nullable copy provenance, creation/source indexes, append-only event table/trigger) then `20261028001043_PreserveIssuedQuoteVersion` (nullable issued-version pointer and restrictive composite FK/index). Apply both through Host.Seed before P3 API/UI activation. Existing versions and numeric/tax snapshots are not rewritten; historical actions remain available in QuoteVersions and no historical transfer is fabricated. Verify fresh migration, repeat no-op and upgrade from P2 in disposable databases. Down removes P3 audit/provenance and issued pointers, so it is only for disposable rehearsal; after use prefer disabling the new UI/actions and forward recovery or coordinated backup restore. No shared/production database was migrated in this task.

Apply additive Crm migrations in an isolated test database first. Deploy API/contract before enabling each frontend slice. Use existing authorized admin access and a capability/route availability mechanism selected under EN-001; do not invent a new configuration framework. Enable clients, then draft quotes, then custom sewing, then verified ready/PDF, then explicitly configured sending. Historical import activates separately after M6. Monitor denied requests, revision conflicts, render/outbox failures and import unresolved counts without leaking payloads.

## Rollback and compatibility

Disable affected actions/routes/worker through the chosen rollout mechanism; preserve read access to committed history. Before external activation, reverse only run-owned rows whose dependencies are verified unmodified, in reverse order, or restore the rehearsed backup. After users edit imported records or sends occur, prefer forward correction and disable the capability: deleting rows or automatic EF down-migration could erase business history. Never roll back Catalog/Reference pricing to repair Crm imports. Restore database and private file store as one coordinated recovery set; do not delete shared/referenced files. Document actual backup IDs, target, command, counts and rehearsal result in task evidence, not invented example success.
