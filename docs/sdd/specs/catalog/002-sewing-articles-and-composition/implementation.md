# Implementation checkpoint — 2026-10-25

## Scope and decisions

The user authorized implementation of Catalog SDD 002 and its Admin consumer, then authorized a local backend commit after focused checks so the frontend can pin the canonical API. Pricing SDD 003 and source-row import remain separate deliveries.

- Existing Identity policies are reused: `CatalogRead` for reads and `CanManageProducts` for writes. New routes are in a separate controller without `AllowAnonymous`; review actor comes from the authenticated name-identifier claim.
- `Product.CompositionVersion` is a long EF concurrency token. Every write requires an explicit `expectedVersion`, including Article creation. Stale writes return 409 and do not replay automatically. Zero is the initial valid version; omitting the JSON field is rejected.
- Sale SKU is globally unique after trimming, with exact case-sensitive comparison; it is distinct from generated Article ID. A Product supports at most 200 Articles and 2,000 materials/operations per collection.
- Model code is edited with the shared composition, nullable and unique when present. Shared composition replacement preserves every Article-specific material line. Article replacement preserves shared lines and all other Articles.
- Fabric uses metres, Accessory uses pieces, and OtherCost uses service. OtherCost is stored separately and always blocks costing until its reference ownership and versioned cost are implemented. Kilogram is reserved and rejected for these current kinds. Existing Reference models do not expose a separate active flag; existence is validated through the existing Reference reader.
- All operation lines are shared and required. Quantities and manual minutes are positive `numeric(18,6)` inputs. Review actor/time are server-owned; changing a line requires the client to explicitly confirm its review state.
- Legacy Product replacement preserves Articles and composition. After new composition exists, legacy norm changes return 409; unchanged norms and ordinary Product edits remain supported. Changing such a Sewing Product to PPE is rejected. Existing fabric-based pricing is retained until the separate pricing delivery.

## Delivery state

Domain, Application, API, EF mappings and an additive migration are implemented. HTTP responses and SQL projection translation are covered by focused tests; exact commands/results are recorded below when verified. No source data has been imported and no migration has been applied to a real database during this delivery.

| Prerequisite/task | State |
| --- | --- |
| EN-001 / TS-007 | Open: target database inventory, frozen source and import dry run are unavailable. |
| EN-002 | Implemented: existing policies, explicit version token and canonical OpenAPI operations. |
| EN-003 | Open for Pricing: OtherCost cost reference; explicitly blocked rather than priced as zero. |
| TS-001 | Domain implemented; focused Article and composition tests. |
| TS-002 | Mapping and migration generated; no-database mapping checks. Empty/upgrade PostgreSQL verification remains required. |
| TS-003–005 | Commands, scoped reads/writes and protected HTTP operations implemented; focused application/API checks. |
| TS-006 | Existing Product behavior covered by focused regression; feature/data map updated. |

## Rollout and remaining gates

Apply the additive migration through the established `Host.Seed` procedure before exposing these routes in a deployed API. Retain the new schema on application rollback. No destructive rollback after data writes is authorized by this delivery.

`BREVIERP_CATALOG_TEST_CONNECTION` is not set and the local PostgreSQL listener is unavailable. Therefore generated SQL/model inspection and query translation do **not** prove execution of the migration, database uniqueness races, rollback, or an end-to-end database write. Before rollout, run the empty-database and existing-schema migration checks and the persistence scenario in a disposable PostgreSQL database.

The `MOD-025` fixture is a future source-data reconciliation case, not evidence of a completed import. Backend review/readiness does not yet mean a price may be published.

## Focused verification

- Initial Red: the first `SewingArticleTests` test failed on the missing Article collection. Domain implementation then passed its focused tests. Application and transport regression tests were added during integration; they are final-state evidence, not a claimed preimplementation Red run.
- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter "FullyQualifiedName~SewingArticleTests|FullyQualifiedName~SewingCompositionHandlerTests|FullyQualifiedName~SewingCompositionContractTests|FullyQualifiedName~ProductCommandHandlerTests|FullyQualifiedName~ProductWriteRequestMapperTests|FullyQualifiedName~Aggregate_contract_is_valid|FullyQualifiedName~Aggregate_operation_ids_are_unique" --verbosity quiet`: 45 passed and one contract parser failure caused by unquoted YAML commas. After quoting those descriptions, reran only the affected contract checks with `--no-build --no-restore --filter "FullyQualifiedName~Aggregate_contract_is_valid|FullyQualifiedName~Aggregate_operation_ids_are_unique|FullyQualifiedName~SewingCompositionContractTests"`: 12/12 passed. Together this verifies all 46 selected cases on the final code/contract state.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~SewingCompositionApiTests|FullyQualifiedName~SewingCompositionPersistenceTests|FullyQualifiedName~SewingCompositionQueryTests|FullyQualifiedName~ProductsApiTests" --verbosity quiet`: 19/19 passed. These use TestServer, EF model inspection/temporary IDs, and SQL translation without a PostgreSQL connection; they do not prove migration execution.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter FullyQualifiedName~CatalogApplicationDependencyTests --verbosity quiet`: 1/1 passed.
- `dotnet build src/Bootstrapper/Host.Api/Host.Api.csproj --no-restore --verbosity quiet`: passed, zero warnings/errors; checks the changed Reference usage adapter and registration boundary.
- Mapping review found EF's default temporary negative keys incompatible with the public typed-ID validation. Internal-only temporary factories and explicit Infrastructure generators fixed the issue; the tracking test covers multiple unsaved child entities.
- Existing package warning `NU1903` for `Microsoft.OpenApi.YamlReader` 2.7.5 appeared in UnitTests. Dependency upgrades are outside this feature.

No full solution suite, browser/E2E run, source import, shared database write or deployment was performed.
