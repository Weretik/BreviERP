# Full local catalogue import evidence — 2026-10-25

## Result and scope

EN-001/TS-007 is complete for the user-authorized initial local transfer. The verified target was brevidb on localhost:5432 (::1). No shared/production database or source workbook was modified. This is a catalogue/composition transfer, not completion of Pricing SDD 003 or historical Calculator data migration.

| Dataset | Source disposition | Persisted result |
| --- | --- | ---: |
| Sewing models | One Product per MOD code | 124 |
| PPE catalogue | One Product and normalized offer/profile per SKU | 119 |
| Products total | Newly allocated Brevi IDs | 243 |
| Sewing sale Articles | Source SKUs retained as strings | 248 |
| Sale positions | 248 Sewing Articles + 119 PPE SKUs, equal to the 367-row price list | 367 |
| Material lines | Shared and Article-specific scope retained | 1,448 |
| Operation lines | Every occurrence retained, including different manual minutes | 1,822 |
| Mapped suppliers | Existing unique exact names reused; source fields reconciled | 43 |
| Mapped fabrics | 83 usable rows; unused empty inactive FAB-087 preserved in provenance | 83 |
| Mapped physical accessories | Service OtherCost excluded from inventory coercion | 40 |
| Mapped operations | 187 source references plus one described manual-source operation | 188 |
| Completed batches | Full source JSON/formula evidence, report and SHA-256 | 1 |
| Source-to-target mappings | Masters, Articles and individual composition lines | 4,173 |

Source SHA-256: 9C8079C1259377BE1ECEE6F85FF50874BFB9B2819EA1B5FFF8ECF6CC48DCBD28.

## Executed operational sequence

1. Verified that local Products contained zero rows and inventoried current Reference/category data.
2. Captured bounded workbook reads, model hyperlinks and six relevant formula tabs in .local/catalog-import/frozen-source.json. This is a frozen read set; no claim of transactional Google snapshot consistency is made.
3. Created .local/catalog-import/before-import.dump with pg_dump -Fc; pg_restore --list read the archive successfully. A restore drill was not run.
4. Ran normal Host.Seed, exit 0. Applied 20261025093328_AddCatalogImportProfiles and 20261025093346_InitializeCatalogImport. Existing ordinary seeders all skipped because their tables already contained data. Earlier 20261024215605_AddSewingComposition remained applied and was not edited.
5. Ran validate, exit 0: 243 Products, 248 Articles, 1,448 material rows, 1,822 operation rows.
6. Ran trial, exit 0: MOD-025 persisted/reconciled with 2 Articles, 10 materials, 13 operations, then all rows were rolled back. The current source includes an additional assembly operation compared with the original 12-operation example.
7. Ran apply, exit 0: full counts above; 408 Reference change messages and 145 row-addressed warnings in the detailed report.
8. After final code organization, reran apply with the identical file, exit 0/status already-applied; counts remained unchanged.
9. Independent read-only SQL confirmed type counts 124/119, 248/1,448/1,822 child counts, one batch, 4,173 source mappings and zero broken material-to-Article parent links. PPE SKU 724 has SupplierPrice NULL and unit пара.

The [runbook](../../../operations/data/catalog-import.md) records the exact Host.Seed command forms. Actual source, backup, report and detailed test log remain local ignored files. The same source payload and import report are also stored in the imports schema of Brevi's database.

## Source exceptions and scope boundary

- Repeated fabric labels receive source-code suffixes and separate Brevi references; they are not merged solely because names coincide.
- A manual operation at Трудовитрати виробів!1820 has no source reference code. Its exact description and 4 manual minutes are preserved under a deterministic technical mapping key.
- OP-155 has no default source norm; every imported usage supplies manual time. The legacy Reference Min field uses zero to represent the unavailable default; original null and source context remain in provenance.
- PPE 724 has no supplier price; no zero price or fabricated legacy markup was introduced.
- Source review flags/dates are separate from Brevi review metadata; imported composition lines remain unreviewed in Brevi.
- Source-only attributes, RU article labels, helper views and derived price columns/formulas are retained in database provenance. They are not all separate editable fields or UI workflows. PPE offers are normalized in ProductCatalogProfiles; legacy PpeProductDetails and calculated/published prices await approved Pricing rules.
- No media blobs, D1 historical transactions/quotes, price rules/publication, changed-snapshot merge, production cutover or browser workflow is claimed.

## Focused verification

All tests below ran against the final relevant source state. No full suite or browser check was requested.

- Red: Import_preserves_metre_accessories_and_repeated_operation_times failed before Domain support (1 failure). Green/regression passed.
- Red: Ambiguous_localized_numeric_text_is_rejected_in_an_unformatted_snapshot failed while numeric parsing accepted a comma as a thousands separator (1 failed, 1 passed). Green rejects ambiguous localized input. An earlier missing namespace compilation error was repaired and is not counted as Red evidence.
- Red: the persisted PPE SKU assertion in Trial_rolls_back_apply_preserves_source_and_retry_is_idempotent failed before SewingCompositionStore checked profiles. Green now rejects reuse of an imported PPE SKU.
- dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter "FullyQualifiedName~CatalogImportSourceTests|FullyQualifiedName~SewingArticleTests|FullyQualifiedName~SewingCompositionHandlerTests|FullyQualifiedName~SewingCompositionContractTests|FullyQualifiedName~Aggregate_contract_is_valid|FullyQualifiedName~Aggregate_operation_ids_are_unique" --verbosity quiet: 34/34 passed, exit 0. Existing NU1903 advisory for Microsoft.OpenApi.YamlReader 2.7.5 remains outside this task.
- dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~CatalogImportPersistenceTests|FullyQualifiedName~SewingCompositionApiTests|FullyQualifiedName~SewingCompositionQueryTests" --verbosity normal: 9/9 passed, exit 0, build zero warnings/errors. BREVIERP_CATALOG_TEST_CONNECTION was set only in the test process from local configuration, without printing credentials. The test created/migrated and removed its own database, checking trial rollback, persisted source values, profile null price, repeated operation times, idempotency and rejection of a different snapshot.
- dotnet build src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-restore --verbosity quiet: passed, zero warnings/errors.
- dotnet ef migrations has-pending-model-changes with CatalogDbContext and CatalogImportDbContext, each using Host.Seed as startup and --no-build under DOTNET_ENVIRONMENT=Production: both report no pending model changes.
- dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter FullyQualifiedName~CatalogApplicationDependencyTests --verbosity quiet: 1/1 passed, exit 0.
- New-document relative-link verification and git diff --check on the task-scoped files passed. Private .local artifacts are absent from Git status. Final file review found one primary type per handwritten production file, separate dataset adapters, and no Jobs project reference from Domain, Application or Host.Api.

## Scoped audit

| Finding | Status and evidence |
| --- | --- |
| AF-IMP-001: metre accessories rejected by Domain/SQL | Fixed: Domain, Application validator and new migration allow Piece/Metre; Domain and PostgreSQL import checks pass. |
| AF-IMP-002: duplicate operation-reference index loses individual manual times | Fixed: occurrences have independent generated IDs; nonunique reference index and persisted-time regression pass. |
| AF-IMP-003: incomplete placeholder/manual source rows block an unqualified full transfer | Fixed by explicit dispositions; no source row is silently lost. Planner tests and actual source validation/trial passed. |
| AF-IMP-004: missing PPE price cannot become a fabricated positive legacy price | Fixed: nullable normalized offer, no synthetic percentages; SQL confirms NULL. |
| AF-IMP-005: imported PPE SKU absent from sewing duplicate check | Fixed: profile lookup added; PostgreSQL Red/Green assertion passes. |
| AF-IMP-006: multi-dataset reference persistence in one large file | Fixed: dedicated Suppliers, Materials and Operations adapters; coordinating writer owns only order and shared SaveChanges. |

The final review covered the import path, target guard, transaction, source dispositions, mapping/reconciliation, migration compatibility and file responsibilities. It is not a repository-wide audit.

