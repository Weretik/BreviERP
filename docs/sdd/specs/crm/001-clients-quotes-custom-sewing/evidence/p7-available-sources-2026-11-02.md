# P7 available-source export and Google workbook reconciliation — 2026-11-02

The latest owner instruction supersedes waiting for a full Calculator export: export and work with all currently available source data, including the approved Google workbook. Missing/truncated rows and absent R2 bytes are still explicit; no source data was fabricated. Identity/custodian assignment remains deferred and automatic deletion remains disabled.

## Verified deliverables

Two agents independently captured the current workbook and queried the existing local catalogue. Coordinator packaged both sources and verified every ZIP member length and SHA-256 after reopening the archive. The private combined ZIP contains 12 source/report files plus EXPORT-MANIFEST.json, 3070030 bytes, SHA-256 9312294bc8be463d93737519181c2efea8831be9ae1e6aad2291a9c8aa3a4493. Native data and private detailed deltas remain outside Git under E:/Development/.local/p7-export/ and E:/Development/.local/p7-source/.

The approved workbook is «Brevi — new references and calculations» (private Google Sheet). Native XLSX includes all 22 tabs, 1362905 bytes, SHA-256 bf8d9b9cb733abcb145c07d80825dc3e5b6b3853304cb237995a2e299397b8e6. All-tab cached values and formulas, source metadata, counts, meaningful delta and checksums were also saved. Google modifiedTime before and after remained 2026-11-02T10:41:02.404Z; this is not a transactional backup claim. The colon removed by the Excel tab-name conversion was mapped back using Google metadata and tab order.

## Current target evidence

Read-only queries against loopback brevidb confirmed the [previous initial import](../../../catalog/002-sewing-articles-and-composition/catalog-import-evidence.md), not just its historical report: 243 active Products (124 Sewing,119 PPE),248 Articles,1448 material lines,1822 operation lines,243 profiles,4173 source mappings and one applied batch dated2026-10-25. Frozen source hash matches that batch.83 mapped fabrics have composition/density; broken material-to-Article links=0. transaction_read_only=on, databaseWrites=0. This does not establish Cloud Run database contents.

## Source changes since the initial import

Compared to the previous source's business columns: suppliers43 unchanged; fabrics84→86 source keys (2new,16changed); accessories41→43 (2new,4changed); models124 unchanged; PPE119 with5changed short descriptions; price-list367 with249changed rows including248displayed prices; material lines1448 with162changed rows; labor1822→1824; additional-material keys84 unchanged.187 operation keys remain;186changes are only computed wage columns. Counts of source keys include placeholders and must not be confused with usable target masters. Computed columns are evidence, not instructions to publish prices.

The current CatalogImport job supports initial import and exact-hash reconciliation. It intentionally rejects a changed snapshot against a nonempty catalogue. These new deltas have been exported and compared, not applied: a controlled update workflow preserving mapped identities, existing edits and historical snapshots is still needed. No catalogue reset, duplicate import, production operation, migration, email, commit or push was performed.
