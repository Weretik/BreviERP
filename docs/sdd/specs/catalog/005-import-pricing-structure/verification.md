## Verified outcome

All seven structural findings are verified. Current navigation is in the [catalogue import card](../../../features/catalog/catalog-import.md), [pricing card](../../../features/catalog/sewing-pricing.md), and [job project guide](../../../../../src/Jobs/README.md). These are current paths; the table above retains the initial evidence.

| Original file | Outcome |
| --- | --- |
| CatalogProductWriter, 138 lines | 24-line coordinator; Products identity/factory/profile, Categories resolution and Composition mapping/writing have named owners. |
| CatalogImportPlanner, 133 lines | 24-line coordinator with Identity and Validation owners; source conventions live under Source. |
| Host.Seed Program | 33-line host lifecycle; Jobs dispatcher and per-scenario command runners preserve CLI/report behavior. |
| SewingPricingVerificationJob, 90 lines | 48-line transaction coordinator, 40-line Article diagnostic reader and 29-line pure parity comparer. |
| Flat pricing types | Catalog calculation/inputs/results/settings and Reference input-version/usage folders; persisted CLR namespaces remain stable. |
| Pricing operations inside CatalogImport | Dedicated src/Jobs/SewingPricing project with one-way import-provenance dependency. |

Catalogue transaction orchestration (86 lines) and pricing bootstrap orchestration (70 lines) remain explicit and cohesive. Small typed records, mappers and material/operation UI rows were kept where their responsibility is already clear; no size-only splitting or generic folder was introduced. The existing Product aggregate's unrelated legacy photo/content code was not rewritten.

## Fresh verification

- dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SewingArticlePricingTests|FullyQualifiedName~SewingArticleTests|FullyQualifiedName~CatalogImportSourceTests|FullyQualifiedName~SewingCompositionHandlerTests" --verbosity quiet: 34/34 passed.
- dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~SewingArticlePricingPersistenceTests|FullyQualifiedName~CatalogImportPersistenceTests|FullyQualifiedName~SewingCompositionQueryTests" --verbosity normal: 5/5 passed. The test process used the local test connection without printing it and created/deleted disposable databases.
- Focused CatalogApplicationDependencyTests and JobDependencyTests: 2/2 passed. The new architectural assertion rejects operational-job dependencies from runtime modules/API.
- Host.Seed and Host.Api builds passed with zero warnings/errors.
- EF has-pending-model-changes for CatalogDbContext, ReferenceDbContext and CatalogImportDbContext with Host.Seed startup: all report no model changes.
- Read-only --pricing-verify 246,247: zero parity differences; review blockers and CanPublish=false retained.
- --catalog-import with --mode validate on the full frozen source: already-applied, preserving 243 Products, 248 Articles, 1,448 materials and 1,822 operations; no import write occurred.
- Conflicting CLI job flags returned exit 1 and the existing exclusivity error before execution.
- No real migrations, normal seeding, bootstrap, import apply or price publication was run. No full suite/browser/production build was run.
- The existing NU1903 advisory for Microsoft.OpenApi.YamlReader 2.7.5 remains outside this structural task.

The frontend follow-up is owned and verified in its repository's SDD005/code audit; no cross-repository links or personal paths are added here.

