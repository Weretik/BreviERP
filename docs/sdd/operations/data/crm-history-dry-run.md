# CRM archive offline dry-run

EN-006/TS-013 inspect a frozen Calculator archive and produce a no-write plan. The runner does not resolve a DbContext, migrate, seed, import, create accounts, send mail, recalculate historical prices or delete archive data. Host startup still registers normal module services from configuration; an unavailable but syntactically valid database connection is sufficient for this mode. Default Host.Seed without a job mode retains its existing migration/seed behavior.

## Command and files

~~~powershell
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --   --crm-history-dry-run=<source-root/freeze.json>   --source-root=<restricted-local-source-directory>   --report=<private-output-directory/report.json>   --staging-ledger=<private-output-directory/ledger.json>
~~~

Use the exact explicit mode. History source-root/staging-ledger hints without that mode are rejected before the normal migration branch. Multiple job modes are rejected, and mode=apply is unsupported. Exit0 means the diagnostic completed, including a blocked source report; it never authorizes an import. Invalid/unreadable inputs fail safely without echoing raw values. Reports/ledger must be new distinct paths outside Git checkouts and source inputs; existing files and linked output directories are rejected.

The freeze JSON is schemaVersion1 with datasetId, databaseFile/databaseSha256, manifestFile/manifestSha256, optional optionsFile/optionsSha256 and optional blobs(sourceFileId,file,sha256,length). Freeze and referenced files stay below explicit sourceRoot; traversal/absolute references, duplicate/unknown control properties and reparse escapes are rejected. Native/options/blob inputs are bounded to64MiB each and verified before use. Native database.json is table-name -> {columns,rows}; manifest.json supplies exact counts/status/flags. The native source files must remain unchanged.

Only12 CRM history entities are staged. Auth/session/OAuth, finance, supplier invoices, old search projections and configuration activation are outside this plan. Raw input files may contain unrelated data; no unrelated payload enters normalized records or output. No remote source connection is made by the reader.

Options contain ownerMappings, exactIdentityMappings, previousStaging, stagingDatasetId, scopeApprovalReference and retentionPolicyReference. Owner keys are SHA256 UTF8(trimmed lowercase source email), or SHA256 UTF8('counterparty:'+opaque source ID) for an explicit customer custodian. Exact external keys are SHA256 of the serialized exact source/type/id tuple, preserving case. Mapping targets are supplied, reviewed metadata, not live-verified records; the future TS-014 must verify target existence/ownership and full target domain inputs before writing. Do not fill an arbitrary user ID. Current owner-confirmed retention reference is owner-no-auto-delete-2026-11-02: no automatic deletion, deletion only by a separate decision.

## Report interpretation

The report contains safe table counts, opaque key hashes, payload/control/options hashes and fixed reason codes. Its stagingLedger array is empty; the optional separate ledger envelope has schemaVersion1,datasetId,entries. It contains only validated opaque source IDs/hashes and remains private. To recheck a previous staging run, copy its entries into options.previousStaging and the same datasetId into options.stagingDatasetId, then freeze the new options checksum. Same keys/hashes are staging no-ops, changed hashes conflict; unresolved owner/graph gates are always revalidated. This is not a receipt of target writes.

Selected SourceRows = wouldImport + alreadyStaged + alreadyMapped + needsReview + excluded + invalid. WouldImport is a preliminary candidate for later controlled validation. AlreadyMapped means an exact operator-supplied identity match, not a database read or new entity. Only explicit CLIENT qualifies (including supported dual roles); supplier/FINANCIAL-only rows are excluded and unknown roles need review. Truncated JSON is quarantined without repair. Missing/invalid graph dependencies propagate through affected records, including cycles. Legacy target Product/Article/template references remain review-only until proven mappings exist. Historical sends are inert. File metadata alone never proves available bytes.

Always databaseWrites=0 and importAuthorized=false. Full/consistent-source flags, table gaps, row issues and missing scope/retention decisions remain visible blockers. A supplied approval label never makes missing history magically complete. The current copy yields a blocked report; [source assessment](../../specs/crm/001-clients-quotes-custom-sewing/evidence/en-006-source-inventory-2026-11-02.md) and [execution evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-013-dry-run-2026-11-02.md) describe its measured limits.

Latest owner instruction2026-11-02: export and work with all available Calculator data and the approved project Google workbook; do not wait for a fuller export. The source remains partial and missing/corrupt records cannot be fabricated. Identity/custodian assignment is deferred. This runner remains read-only. Retention remains no automatic deletion, with deletion only by a separate decision.
