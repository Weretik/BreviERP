# TS-008C structural verification — 2026-10-26

Scope: AF-IR01..03. Extraction/moves only: private composition field comparers, independent settings reader and capability folders. Public namespaces/report shape, source comparisons, evaluation order, read-only repeatable-read transaction and local target guard are preserved. The job project and runtime dependencies are unchanged. No artificial Red is required for moving existing behavior; existing review tests are the regression.

## Fresh checks

- Scoped `dotnet format whitespace src/Jobs/SewingPricing/SewingPricing.csproj --no-restore --include` the Review production files: passed.
- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter PricingInputReviewTests --verbosity quiet`: 3 passed, covering numeric normalization, missing/changed fields and local-target rejection. Existing NU1903 OpenApi.YamlReader warning remains outside this refactor.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter PricingInputReviewTests --verbosity quiet`: 1 passed against an isolated PostgreSQL fixture. Baseline matches, changed/deleted lines remain detected, and draft review/approval flags remain untouched.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter JobDependencyTests --verbosity quiet`: 1 passed.
- `dotnet build src/Bootstrapper/Host.Seed/Host.Seed.csproj --no-restore --verbosity quiet`: passed, 0 warnings/errors.
- Explicit local input review with existing frozen settings/PPE paths and report `.local/pricing/ts008-structure-review.json`: exit 0, 3,836 matched, 0 different, 2 held. Exact JSON object comparison against `.local/pricing/ts008-input-review.json`, ignoring only CheckedAt: identical. This includes every field, selected version, status, hold and source hash.
- Source inspection confirms no MOD-061/452/453/DR-C01-specific branch in production Review code. Those Articles follow the existing common Domain/Application pricing behavior; the documented human decision records a source discrepancy, not a special runtime formula or manual price override.
- Changed-document local links and final `git diff --check`: passed.

AF-IR01..03 are verified; no open structural finding remains within this scope. Previous source-review reports and DR-C01 evidence are retained unchanged. No import, migration, approval, publication, full suite, browser check, production deployment or commit was performed. Earlier unrelated dirty work was preserved. The structural report is ignored under `.local/pricing/`.
