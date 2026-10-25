# EN-004 / TS-004 delivery evidence

Date: 2026-10-25. Scope: [publication implementation](publication-implementation.md). Backend implementation and local migrations only; no frontend cutover, PPE change or remote deployment.

Subsequent behavior-preserving decomposition is recorded in the [structure audit](structure/audit.md) and [fresh verification](structure/verification.md). Use the [current feature map](../../../features/catalog/sewing-pricing.md) for source paths after that refactoring.

## Scenarios and tests

| Scenario | Evidence |
| --- | --- |
| SC-P01/02/03 | `SewingPricingPublicationTests.Saved_calculations_are_immutable_and_only_current_reviewed_inputs_can_publish_with_history`: blocked snapshot retained, retry identity, cross-Article request conflict, full settings/composition/reference retention, cost/settings/composition staleness, publication retry and prior price retention. |
| SC-P02 concurrency | `SewingPricingConcurrencyTests.Publication_waits_for_a_concurrent_cost_edit_and_checks_its_committed_version`: a separate write transaction holds the advisory lock; publication waits, then returns conflict against the committed new version. |
| SC-P04 | `ReferencePricingRevisionTests.Reference_edits_append_atomically_and_reject_stale_and_future_revisions`: name-only edit, all three numeric owners, stale/future rollback, synchronous bypass guard and exact eight-decimal norm round trip. `OtherCostPricingRevisionTests` covers retries, invalid values, expected version and retained prior cost. |
| SC-P05 | `SewingPricingApiTests`: HTTP 401/403, claim-derived actor, scoped route IDs, 409 publication, tier JSON and incomplete settings rejection. `SewingPricingContractTests`: six named Catalog operations/policies and sixteen schema/property comparisons. |
| Migration/transaction | `SewingPricingMigrationTests`: current catalogue survives baseline-to-history upgrade, uncommitted insert rolls back, wrong Product/Article pair fails its composite FK. Reference test upgrades the existing norm column and preserves its old value. Every fixture also migrates an empty disposable PostgreSQL database. |

Red evidence: the first five `SewingPricingContractTests` failed for absent pricing operations (5 failed, 0 passed); after implementation they passed. Generated migrations and PostgreSQL snapshot fixtures needed the schema/Application prerequisites before meaningful persistence execution; their final integration evidence is recorded separately, not presented as a compilation-error Red.

## Focused checks

- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter "SewingPricingContractTests|SewingArticlePricingTests|SewingArticleTests|SewingCompositionHandlerTests|CatalogImportSourceTests" --verbosity quiet`: 57 passed, exit 0.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter "SewingPricing|ReferencePricingRevisionTests|OtherCostPricingRevisionTests|SewingArticlePricingPersistenceTests|CatalogImportPersistenceTests|SewingCompositionApiTests" --verbosity quiet`: 15 passed, exit 0. `BREVIERP_CATALOG_TEST_CONNECTION` was set only for the process, without printing it; fixtures created/dropped their own databases.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter "CatalogApplicationDependencyTests|JobDependencyTests" --verbosity quiet`: 2 passed, exit 0.
- `dotnet build src/Bootstrapper/Host.Api/Host.Api.csproj --no-restore --verbosity quiet`: exit 0, zero warnings/errors. Host.Seed built successfully during migration generation and local jobs.
- `dotnet ef migrations has-pending-model-changes --no-build --context <CatalogDbContext|ReferenceDbContext> --project <owning Infrastructure project> --startup-project src/Bootstrapper/Host.Seed/Host.Seed.csproj`: both report no model changes, exit 0.
- Navigation check: 47 Markdown links, seven aggregate pricing paths and Reference write-security declarations verified. `git diff --check`: exit 0.
- Existing `SessionOpenApiContractTests` aggregate validation/reference resolution and operation-ID uniqueness checks: 2 passed, exit 0 (`dotnet test tests/UnitTests/UnitTests.csproj --no-build --filter "FullyQualifiedName~Aggregate_contract_is_valid_and_all_references_resolve|FullyQualifiedName~Aggregate_operation_ids_are_unique_and_session_operations_are_exposed" --verbosity quiet`).
- `dotnet format whitespace BreviERP.sln --no-restore --include <affected feature files> --verbosity quiet`: exit 0; only the selected files were formatted.

The existing NU1903 advisory for Microsoft.OpenApi.YamlReader 2.7.5 is unrelated and unchanged. No full suite, coverage, browser check or frontend production build was run.

## Local rollout and recovery

Verified target: loopback PostgreSQL, database brevidb. A pre-publication custom-format pg_dump (425,626 bytes) and its successful pg_restore catalogue listing are retained in ignored local storage. This validates backup readability, not a full restore drill. Jobs run through Host.Seed with DOTNET_ENVIRONMENT=Production; API does not migrate.

- Catalog `20261025150231_AddSewingPriceHistory`: new calculation/publication tables, UUID/request uniqueness and restrictive history FKs.
- Catalog `20261025151551_EnforceSewingCalculationArticleOwnership`: composite parent ownership. The earlier migration had already been applied locally, so it was preserved and this separate additive constraint migration was created.
- Reference `20261025151946_PreservePricingOperationPrecision`: numeric(18,2) to numeric(24,8), retaining all original integer digits. EF's generic alteration warning was reviewed: Up expands fractional precision; Down can round subsequent values.

No real Article was bulk-reviewed, calculated for publication or published. Imported source values and review flags remain the baseline. Production rollback retains history and precision and disables consumer rollout; do not drop populated history or narrow edited norm precision.

Final `dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-launch-profile` with DOTNET_ENVIRONMENT=Production completed with exit 0. SQL confirmed all three migration IDs, 243 Products, 248 Articles, 1,448 materials, 1,822 operation occurrences and 312 Reference input versions. Real saved calculations/publications and reviewed material/operation counts are all zero. No catalogue reimport or price publication occurred.

Read-only `dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --pricing-verify 246,247 --report <ignored-local-report>` completed with exit 0. Both Articles retain zero source-arithmetic differences, strict review blockers and CanPublish=false. Source parity and safe publication are separate checks.

## Scoped architecture audit

| Finding | Severity / owner | Resolution and evidence |
| --- | --- | --- |
| AF-P01: mutable inputs did not append versions | High / Reference | `DataBase/ReferenceDbContext.PricingWrites.cs` and `Pricing/InputVersions/Writing/ReferencePricingChangeAppender.cs` capture values atomically; stale/future and rollback tests verified. |
| AF-P02: anonymous writes could approve pricing inputs | High / Reference API | Fabric/accessory/operation write actions require CanManageProducts; public GET retained. Machine contracts and HTTP authorization checks agree. |
| AF-P03: saved calculations need stable input evidence and race protection | High / Catalog pricing | `Features/Pricing/Evaluation` owns shared evaluation; History store owns persistence/locks. Host integration adapter enlists Reference reads. Snapshot, concurrent-write and stale-publication tests verified. |
| AF-P04: legacy norm column rounded fractional minutes | Medium / Reference persistence | Separate precision migration; exact 7.12345678 round trip verified. |
| AF-P05: independent parent FKs allowed a mismatched Product/Article | High / Catalog persistence | Composite FK migration; PostgreSQL rejects mismatched ownership. |

New production types have separate files and scenario/topic folders. API controllers bind/authorize/map results; Application owns scenarios; Domain retains arithmetic/validation; Infrastructure owns version/history persistence. Cross-module transaction enlistment stays in the Host composition adapter, not module controllers. No runtime module gained a dependency on jobs. No unresolved finding remains in this backend scope. TS-005 frontend/consumer synchronization and TS-006 PPE remain open.
