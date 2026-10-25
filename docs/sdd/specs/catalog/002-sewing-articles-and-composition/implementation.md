# Implementation checkpoint — 2026-10-25

## Scope and decisions

The user authorized implementation of Catalog SDD 002 and its Admin consumer, then authorized a local backend commit after focused checks so the frontend can pin the canonical API. The subsequent full local source-row import is recorded in the [import evidence](catalog-import-evidence.md). Pricing SDD 003 remains a separate delivery.

- Existing Identity policies are reused: `CatalogRead` for reads and `CanManageProducts` for writes. New routes are in a separate controller without `AllowAnonymous`; review actor comes from the authenticated name-identifier claim.
- `Product.CompositionVersion` is a long EF concurrency token. Every write requires an explicit `expectedVersion`, including Article creation. Stale writes return 409 and do not replay automatically. Zero is the initial valid version; omitting the JSON field is rejected.
- Sale SKU is globally unique after trimming, with exact case-sensitive comparison; it is distinct from generated Article ID. A Product supports at most 200 Articles and 2,000 materials/operations per collection.
- Model code is edited with the shared composition, nullable and unique when present. Shared composition replacement preserves every Article-specific material line. Article replacement preserves shared lines and all other Articles.
- Fabric uses metres, Accessory uses pieces or metres, and OtherCost uses service. The legacy composition view conservatively directs OtherCost to the new Article pricing calculation, where Reference-owned versions now resolve its unit cost. Kilogram is reserved and rejected for these current kinds. Legacy Reference models do not expose an active flag; the new pricing versions retain source activity independently.
- All operation lines are shared and required. Quantities and manual minutes are positive `numeric(18,6)` inputs. Review actor/time are server-owned; changing a line requires the client to explicitly confirm its review state.
- Legacy Product replacement preserves Articles and composition. After new composition exists, legacy norm changes return 409; unchanged norms and ordinary Product edits remain supported. Changing such a Sewing Product to PPE is rejected. Existing fabric-based pricing is retained until the separate pricing delivery.

## Delivery state

Domain, Application, API and EF mappings are implemented. HTTP responses and SQL projection translation are covered by focused tests. The additive migrations and full local catalogue import were subsequently executed on 2026-10-25 under direct user authorization; see the original schema rollout below and the [import evidence](catalog-import-evidence.md).

| Prerequisite/task | State |
| --- | --- |
| EN-001 / TS-007 | Complete for the authorized local initial transfer: verified empty target, frozen source, backup, transactional trial/full import and idempotent rerun. |
| EN-002 | Implemented: existing policies, explicit version token and canonical OpenAPI operations. |
| EN-003 | Resolved by the initial [Pricing slice](../003-sheet-pricing-parity/sewing-implementation.md): Reference owns OtherCost code/unit/value versions. The legacy composition view conservatively requires the versioned pricing check. |
| TS-001 | Domain implemented; focused Article and composition tests. |
| TS-002 | Mapping checks, existing local database upgrade and disposable PostgreSQL empty-schema/import persistence verification passed. Broader concurrency races and production rollout remain separate. |
| TS-003–005 | Commands, scoped reads/writes and protected HTTP operations implemented; focused application/API checks. |
| TS-006 | Existing Product behavior covered by focused regression; feature/data map updated. |

## Rollout and remaining gates

Apply the additive migration through the established `Host.Seed` procedure before exposing these routes in a deployed API. Retain the new schema on application rollback. No destructive rollback after data writes is authorized by this delivery.

At the initial checkpoint the local PostgreSQL listener was unavailable. Later checks used `BREVIERP_CATALOG_TEST_CONNECTION` for disposable databases and verified migration execution, transactional rollback and persisted import data. Database uniqueness races, a full browser workflow and shared/production rollout are not claimed.

The `MOD-025` fixture and full local import are now reconciled; the current source has 13 operations instead of the original 12. Backend review/readiness does not yet mean a price may be published.

## Focused verification

- Initial Red: the first `SewingArticleTests` test failed on the missing Article collection. Domain implementation then passed its focused tests. Application and transport regression tests were added during integration; they are final-state evidence, not a claimed preimplementation Red run.
- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter "FullyQualifiedName~SewingArticleTests|FullyQualifiedName~SewingCompositionHandlerTests|FullyQualifiedName~SewingCompositionContractTests|FullyQualifiedName~ProductCommandHandlerTests|FullyQualifiedName~ProductWriteRequestMapperTests|FullyQualifiedName~Aggregate_contract_is_valid|FullyQualifiedName~Aggregate_operation_ids_are_unique" --verbosity quiet`: 45 passed and one contract parser failure caused by unquoted YAML commas. After quoting those descriptions, reran only the affected contract checks with `--no-build --no-restore --filter "FullyQualifiedName~Aggregate_contract_is_valid|FullyQualifiedName~Aggregate_operation_ids_are_unique|FullyQualifiedName~SewingCompositionContractTests"`: 12/12 passed. Together this verifies all 46 selected cases on the final code/contract state.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~SewingCompositionApiTests|FullyQualifiedName~SewingCompositionPersistenceTests|FullyQualifiedName~SewingCompositionQueryTests|FullyQualifiedName~ProductsApiTests" --verbosity quiet`: 19/19 passed. These use TestServer, EF model inspection/temporary IDs, and SQL translation without a PostgreSQL connection; they do not prove migration execution.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter FullyQualifiedName~CatalogApplicationDependencyTests --verbosity quiet`: 1/1 passed.
- `dotnet build src/Bootstrapper/Host.Api/Host.Api.csproj --no-restore --verbosity quiet`: passed, zero warnings/errors; checks the changed Reference usage adapter and registration boundary.
- Mapping review found EF's default temporary negative keys incompatible with the public typed-ID validation. Internal-only temporary factories and explicit Infrastructure generators fixed the issue; the tracking test covers multiple unsaved child entities.
- Existing package warning `NU1903` for `Microsoft.OpenApi.YamlReader` 2.7.5 appeared in UnitTests. Dependency upgrades are outside this feature.

The initial verification did not run a full solution suite, browser/E2E, source import, shared database write or deployment. The subsequent authorized local source import is recorded separately in [import evidence](catalog-import-evidence.md).

## Local migration evidence — 2026-10-25

The user explicitly requested the local migration through the project's job. The verified target was `brevidb` on `localhost:5432` (server address `::1`). The existing PostgreSQL 18 cluster was started with `pg_ctl`; Windows service control was unavailable to the current process. PostgreSQL remains running for local development.

- Before execution, `public.__EFMigrationsHistory` contained 14 migrations and did not contain `20261024215605_AddSewingComposition`.
- Executed `dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-launch-profile`; exit code 0. The job completed Identity, Reference, Accounting and Catalog migration runners. Every registered seeder reported a skip because its table already contained data.
- Read-only SQL verification confirmed the new migration history row; `catalog.SewingArticles`, `catalog.SewingMaterialLines`, and `catalog.SewingOperationLines`; and `Products.CompositionVersion` (`bigint`, required) plus `Products.ModelCode` (nullable character varying).
- This initial schema-only run performed no source import or product insertion. The subsequent local catalogue transfer and disposable-database checks are recorded in [import evidence](catalog-import-evidence.md). Shared environment migration and deployment remain outside scope.
