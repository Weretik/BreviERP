# Full local catalogue import — EN-001 / TS-007

## Authorized scope

The user authorized preparation and full import of the current Google workbook catalogue into the local Brevi database. This revision supersedes TS-007's dry-run-only scope. Pricing SDD 003 is independent: importing identity, composition and supplier offers does not publish prices.

## Rules and acceptance

- R-IMP-001: Freeze a bounded workbook snapshot with provenance and SHA-256 before writes. Preserve all fetched source columns in database provenance; operational records use Brevi-owned IDs and relationships.
- R-IMP-002: Validate all model, SKU, material and operation relationships before writing. Never invent a missing price, approval or supplier identity. Use the existing unspecified-supplier record only when the source supplier is absent, and report every such row.
- R-IMP-003: On an initial import the Product catalogue must be empty. Reference records are matched by a unique exact name; source-backed fields and changed values are recorded in the import transaction and report. Ambiguous matches abort. No unrelated reference record is deleted.
- R-IMP-004: A local job supports validate, trial (one model, rolled back), and apply. All writes across Catalog, Reference and import provenance share one PostgreSQL transaction. Acquire a transaction advisory lock before target checks. Failure rolls back the whole batch.
- R-IMP-005: Reapplying the same snapshot is a no-op; a different snapshot against a populated catalogue is rejected. No merge or destructive reset is implicit.
- R-IMP-006: Keep each source operation occurrence, including repetitions with different manual minutes. Accessory quantities may be pieces or metres. Service lines remain OtherCost and block calculation until Pricing owns their cost rule.
- R-IMP-007: Preserve PPE sale SKU and nullable supplier price in a normalized Product profile. Do not synthesize legacy pricing percentages or published prices. Preserve source names, seasonal/active/review metadata and supplier links. Duplicate product names receive a deterministic SKU suffix, recorded in the report.

| Scenario | Acceptance and verification |
| --- | --- |
| SC-IMP-001 | Invalid or incomplete source fails before writes, with row-addressed diagnostics; planner unit tests. |
| SC-IMP-002 | Trial MOD-025 creates 2 Articles, 10 materials and 12 operations, checks persisted relationships, and rolls everything back; PostgreSQL integration/job evidence. |
| SC-IMP-003 | Apply imports every sewing model, sale article and PPE identity, all composition lines and required references; source-to-target reconciliation uses IDs and values, not counts alone. |
| SC-IMP-004 | A repeated apply adds no records; a changed source or populated unrelated catalogue is rejected; PostgreSQL integration evidence. |
| SC-IMP-005 | Missing PPE cost stays null and repeated operations retain their own quantities/times; focused domain and persistence checks. |

## Structure and data ownership

The operational import library under src/Jobs/CatalogImport owns source parsing, planning, reconciliation and transactional adapters; Host.Seed is its command entry point. It is not referenced by Host.Api or Domain/Application. Catalog and Reference domain factories retain their invariants. Catalog owns Product profiles and composition; Reference owns suppliers/materials/operations; the job owns import batches and source-ID mapping in the imports schema.

Product IDs remain existing typed int IDs, allocated under an exclusive table lock because the legacy schema uses ValueGeneratedNever. Article/line IDs remain generated long IDs. Import provenance uses scalar IDs only inside Infrastructure records and does not introduce primitive Domain foreign keys.

## Rollout and recovery

Apply additive migrations through Host.Seed before importing. Validate, then trial, then apply against a verified loopback PostgreSQL target. A failed or trial transaction changes no operational rows (PostgreSQL sequences may advance). Successful recovery uses the pre-import database backup; do not automatically delete imported rows after users edit them. Raw snapshots, backups and detailed contact-bearing reports are local ignored artifacts; do not commit them.

## Evidence

Implementation commands, counts, changes and remaining limitations are recorded after verification in the implementation checkpoint.
