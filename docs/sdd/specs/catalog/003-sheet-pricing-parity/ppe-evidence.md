# TS-006 evidence — 2026-10-25

Backend implementation checkpoint; frontend integration remains in progress. No production deployment or real-data approval is claimed.

- Red: `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter PpePricingTests --verbosity quiet` — 5 failed / 1 passed against the noncalculating baseline. Failures were missing tier outputs/overflow result, not compilation.
- Green: same filter — 6 passed after Domain implementation.
- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'PpePricing|Aggregate_contract_is_valid_and_all_references_resolve' --verbosity quiet` — 25 passed. Covers serialization/schema parity, operation policies, input bounds/precision, approval distinction, arithmetic and precedence.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter PpePricing --verbosity quiet` with local test connection — 2 passed in isolated PostgreSQL; draft bootstrap/replay, changed-source rejection, immutable history, actor-bound idempotency, stale version conflict, exact override prices, HTTP authentication/claim actor and repeated query IDs. Fresh regression including the Catalog-only upgrade path and neighboring sewing API: 6 passed. A first upgrade-test run mistakenly selected a Reference migration from the shared history table; the fixture now selects only Catalog-owned migration IDs.
- Generated `AddPpePricingVersions` is additive: two Catalog tables, Product FK with Restrict, indexed scope and unique public/request UUIDs. No applied migration edited. Empty database path is exercised by the fixture; upgrade path uses the preceding migration and retains existing imported Products.

Initial local boundary review: API binds/authorizes/maps only; Application owns read/approve scenarios; Domain arithmetic has no dependencies on transport/storage/jobs; Infrastructure owns persistence and bounded projections. The import planner maps source values only. Source capture files stay ignored; secrets are not included. No unstructured Common/Helpers folder or additional module dependency was introduced.

Known existing warning: Microsoft.OpenApi.YamlReader 2.7.5 NU1903. It predates this feature and was not suppressed. Full suite, browser and production build are outside the requested focused verification.

Final checkpoint checks: `PpePricing|SewingPricingContractTests|Aggregate_contract_is_valid_and_all_references_resolve` — 50 passed; affected C# whitespace formatted. Fresh Host.Seed build passed with zero warnings/errors; `dotnet ef migrations has-pending-model-changes --no-build --context CatalogDbContext --project src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Catalog.Infrastructure.csproj --startup-project src/Bootstrapper/Host.Seed/Host.Seed.csproj` reported no changes. Source files are ignored. Only TS-006 files are included in the authorized local checkpoint; pre-existing SDD changes remain unstaged.
