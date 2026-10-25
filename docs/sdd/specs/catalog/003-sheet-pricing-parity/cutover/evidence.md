# TS-007 execution evidence — 2026-10-25

Backend checkpoint verified; frontend contract repin/final checks are in progress. Business data was read only.

- Red: ProductPricingCutoverTests expected deprecated metadata retail price 0 but observed 150. This was reproduced through the public mapper before removal.
- Green: ProductPricingCutoverTests + ProductAdminListReadModelTests, 3 passed.
- Focused UnitTests filter `ProductPricingCutoverTests|ProductAdminListReadModelTests|PpePricing|SewingArticlePricingTests|Aggregate_contract_is_valid_and_all_references_resolve`: 46 passed before the final diagnostic-report extension; final repeat also passed 46, followed by 2 passing Catalog/Application and job dependency checks.
- IntegrationTests filter `PpePricing|SewingPricing|GetAdminProductsSpecTests|ProductsApiTests`: 24 passed, including immutable saved/publication history, read-only diagnostics, idempotent input writes, authenticated APIs and real PostgreSQL list projection. Isolated test databases are created and dropped by fixtures.
- Fresh Host.Seed build passed. Both explicit verification modes passed on the verified local catalogue; see [source reconciliation](reconciliation.md). Detailed reports retain source versions and expected/actual/deltas; CanPublish is false.
- EF `has-pending-model-changes` reported no change. No migrations/imports/seeders or approval/publication commands were run on the business database.

Commands from repository root:

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'ProductPricingCutoverTests|ProductAdminListReadModelTests|PpePricing|SewingArticlePricingTests|Aggregate_contract_is_valid_and_all_references_resolve' --verbosity quiet
dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter 'PpePricing|SewingPricing|GetAdminProductsSpecTests|ProductsApiTests' --verbosity quiet
dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter 'CatalogApplicationDependencyTests|JobDependencyTests' --verbosity quiet
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --pricing-verify 246,247,350,374,386,472 --report .local/pricing/ts007-sewing.json
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-build --no-launch-profile -- --ppe-pricing-verify true --report .local/ppe-pricing/ts007-parity.json
```

Use DOTNET_ENVIRONMENT=Production for the operational host as in existing local instructions. Integration checks use the configured local test connection only; no credentials are documented. The known pre-existing NU1903 warning for OpenApi.YamlReader was not suppressed. No full suite, browser, production build or deployment was performed. Quote snapshots and Storefront API adoption are not implemented by TS-007.
