# TS-007 structural verification — 2026-10-25

Scope: AF-R04, responsibility extraction only. The public job/report API, formula version, read-only guard, source selection, chunk limit and classification remain unchanged. No runtime project references, HTTP contracts or schema changes were introduced. Prior uncommitted TS-006 decomposition and unrelated documentation changes were preserved.

- `dotnet format whitespace src/Jobs/CatalogImport/CatalogImport.csproj --no-restore --include` the four changed/new Verification production files: passed.
- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'PpePricingParityTests|ProductPricingCutoverTests|ProductAdminListReadModelTests' --verbosity quiet`: 5 passed, including every-tier/missing-source classification and retired price metadata.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter 'PpePricingVerificationTests' --verbosity quiet`: 1 passed against an isolated local PostgreSQL fixture, including no created/approved input versions.

Existing package/nullable/compiler warnings outside the changed files were not suppressed. No full suite, import, migration, production deployment or business-data write was run. Job dependency and final navigation/diff results are recorded below.

Job dependency regression (`dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter JobDependencyTests --verbosity quiet`): 1 passed. Local document links and `git diff --check` passed. AF-R04 is verified. No commit was created for this refactor.
