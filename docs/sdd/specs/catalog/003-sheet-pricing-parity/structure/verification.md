# Publication structure verification

Scope and finding IDs: [audit](audit.md). This is behavior-preserving refactoring of the implemented pricing stage. Existing scenarios provide characterization; no new business behavior or artificial failing test is introduced for file moves and adapter extraction.

## Before/after characterization

Before refactoring:

- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter "SewingPricingContractTests|SewingArticlePricingTests" --verbosity quiet`: 35 passed, exit 0.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter "SewingPricing|ReferencePricingRevisionTests|OtherCostPricingRevisionTests|SewingArticlePricingPersistenceTests" --verbosity quiet`: 9 passed, exit 0.

After refactoring, the same filters passed again: 35 Unit/contract checks and 9 Integration checks, exit 0. PostgreSQL fixtures created and removed separate test databases using the process-local BREVIERP_CATALOG_TEST_CONNECTION; the connection was not printed.

The integration checks cover saved/blocked/stale calculations, immutable snapshots, publication retries/history, concurrent cost editing, transaction rollback, composite Article ownership, all three numeric Reference sources, OtherCost/settings revisions, precision and HTTP access. An intermediate compilation failure exposed a remaining diagnostic-job import of input resolution from its old query namespace; the import was corrected before the final passing run. This was a refactoring correction, not a behavioral Red test.

Additional focused checks:

- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter "CatalogApplicationDependencyTests|JobDependencyTests" --verbosity quiet`: 2 passed, exit 0.
- `dotnet build src/Bootstrapper/Host.Api/Host.Api.csproj --no-restore --verbosity quiet`: exit 0, zero warnings/errors.
- `dotnet build src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-restore --verbosity quiet`: exit 0, zero warnings/errors; confirms moved Application code and diagnostic job compile together.
- `dotnet ef migrations has-pending-model-changes --no-build --context <CatalogDbContext|ReferenceDbContext> --project <owning Infrastructure project> --startup-project src/Bootstrapper/Host.Seed/Host.Seed.csproj`, with DOTNET_ENVIRONMENT=Production: both report no model changes, exit 0.
- `dotnet format whitespace BreviERP.sln --no-restore --include <affected pricing source paths> --verbosity quiet`: exit 0.
- Scoped navigation check: 34 Markdown links resolved. No references to the removed combined history store/interface remain in src/tests, and shared pricing Contracts no longer depend on feature namespaces. `git diff --check`: exit 0.

The pre-existing NU1903 advisory for Microsoft.OpenApi.YamlReader 2.7.5 remains unchanged. No full suite, coverage, browser/frontend checks, catalogue import, bootstrap, real publication, real database migrations or commit was run.

## Responsibility verification

| Finding | Current responsibility and evidence |
| --- | --- |
| AF-PS01 | Calculation store: 37 lines; publication store: 47; transaction boundary: 27; snapshot serializer: 15. Each has one primary type/responsibility. Read handlers receive their own storage contract; save/publish explicitly retain transaction ownership. Records/configurations remain alongside their persistence owner. |
| AF-PS02 | Shared snapshot belongs to Contracts; evaluator/input resolution belong to Evaluation. Preview, saved calculations, publications and settings have distinct Sewing scenario directories. Contract property tests and calculation/source fixtures passed. |
| AF-PS03 | Tracked change discovery: 38 lines; revision appender: 31; tracked value descriptor: 9. Existing typed owner predicates and original-value comparisons are preserved; integration tests cover all three owners and stale/future rejection. |
| AF-PS04 | HTTP entry points and requests grouped under Sewing pricing. JSON fields, operation names, claims and policies are unchanged; HTTP and contract tests passed. |

Persisted Domain/Infrastructure record CLR identities, EF mappings, API routes and formula arithmetic are unchanged. Empty source folders were removed after validated in-repository moves. No new runtime project or cross-module business dependency was introduced; Catalog and Reference retain their ownership boundaries.
