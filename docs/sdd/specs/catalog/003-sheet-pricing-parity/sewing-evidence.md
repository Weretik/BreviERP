# Sewing pricing execution evidence — 2026-10-25

Historical evidence for the initial engine slice. See [publication evidence](publication-evidence.md) for the subsequently implemented saved-result, input-edit and publication APIs.

## Completed scope

EN-001/002/003 and TS-001/002/003 are implemented for the initial sewing calculation slice in [sewing-implementation](sewing-implementation.md). No claim is made that publication/API cutover or PPE pricing is complete.

| Item | Verified result |
| --- | --- |
| Local target | brevidb, localhost:5432 |
| New settings table | catalog.SewingPricingSettingsVersions, one initial approved source-baseline row |
| New Reference table | reference.PricingInputVersions, 312 rows: 83 fabrics, 40 physical accessories, 1 OtherCost, 188 operation references |
| Settings source SHA-256 | E34D5EC770FC9941E5EBDC69667A211D26FAED5454009E44D26919D6A0FE91F9 |
| Catalogue source SHA-256 | 9C8079C1259377BE1ECEE6F85FF50874BFB9B2819EA1B5FFF8ECF6CC48DCBD28 |
| Formula implementation | sewing-sheet-v1 |
| Source evidence | normalized setting fields plus complete settings/formula JSON; Reference versions retain source code/hash/date |
| Rerun | already-bootstrapped, same settings ID 1 and 312 Reference versions |
| Catalogue preservation | No Product/Article/composition data or review flag changed by pricing bootstrap/verification |

Applied migrations through Host.Seed: 20261025102032_AddSewingPricingSettingsVersions and 20261025102037_AddReferencePricingInputVersions. A pre-change backup was written to .local/pricing/before-pricing-20261025-103118.dump. Initial normal seeders reported skips. The earlier catalogue migrations were not edited.

## Actual local parity

The current source has 13 MOD-025 operation occurrences totaling 84.84 minutes. Comparison checks all eleven source components/tier outputs, not only the final sum. Unrounded breakdown differences must be at most 0.00000001; displayed prices match exactly at two decimals.

| Article | Material total including OtherCost | Full cost displayed | 1–9 | 10–40 | 41+ | Differences |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 246 | 277.99 | 754.67 | 1169.74 | 996.17 | 943.34 | 0 |
| 247 | 544.39 | 1021.07 | 1582.66 | 1347.81 | 1276.34 | 0 |

Both strict calculations correctly retain 22 review blockers from the imported composition. The separate arithmetic report has CanPublish=false. No zero-as-price result, automatic approval, public price record or frontend cutover occurred.

Executed with exit 0: normal Host.Seed migrations, --pricing-bootstrap, --pricing-verify 246,247, and repeated --pricing-bootstrap. Exact command forms are in the [runbook](../../../operations/data/sewing-pricing.md). Reports are .local/pricing/bootstrap.json, bootstrap-rerun.json and parity.json.

## Tests and repairs

- Red on the initial inherited division policy: 2 source-parity cases failed and 5 other selected cases passed. After changing cutting/master/manager to multiplication, all 7 initial cases passed. Subsequent cases cover repeated manual-only operations, quantity, zero rates, currency rounding, invalid fraction magnitudes, inactive/unapproved costs and storage precision.
- PostgreSQL verification first exposed Npgsql failure mapping arrays of nullable typed IDs. Using nullable arrays still failed. The effective-version adapter now uses scalar int/string array parameters through FromSqlInterpolated, retaining SQL filtering, no-tracking projection and a correlated latest-version query. No unbounded client-side history filtering was substituted.
- The synthetic import fixture originally omitted Reference activity flags. The strict calculation correctly blocked it. Explicit active flags were added to the synthetic fixture rather than weakening runtime validation.
- The final integration lifecycle verifies empty-schema migrations, source bootstrap/rerun, composition-review gating, persisted source quantities/manual times, effective-date reads, immutable older versions, ambiguous cost versions and ambiguous settings versions. The catalogue-import lifecycle is rerun because its shared fixture gained explicit Reference activity flags.
- Final Unit command: dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter "FullyQualifiedName~SewingArticlePricingTests|FullyQualifiedName~SewingArticleTests|FullyQualifiedName~CatalogImportSourceTests|FullyQualifiedName~ReferenceProductUsageTests|FullyQualifiedName~PricingReferenceQueryHandlerTests" --verbosity quiet: 25/25 passed, exit 0.
- Final Integration command with BREVIERP_CATALOG_TEST_CONNECTION set only in the process: dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~SewingArticlePricingPersistenceTests|FullyQualifiedName~CatalogImportPersistenceTests|FullyQualifiedName~SewingCompositionQueryTests" --verbosity normal: 5/5 passed, exit 0. Includes an equivalent non-UTC offset instant, verifying normalized effective selection.
- dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter FullyQualifiedName~CatalogApplicationDependencyTests --verbosity quiet: 1/1 passed, exit 0.
- dotnet build for Host.Api and Host.Seed separately with --no-restore --verbosity quiet: both passed, zero warnings/errors.
- dotnet ef migrations has-pending-model-changes for CatalogDbContext and ReferenceDbContext with Host.Seed startup, --no-build and DOTNET_ENVIRONMENT=Production: both report no pending changes.
- Final pricing-verify and repeated pricing-bootstrap against the existing local DB passed again on the last compiled state. SQL independently confirmed 243 Products, 248 Articles, 1,448 material rows, 1,822 operation rows, 1 settings version, 312 Reference versions and zero automatically reviewed material/operation rows. FUR-032 is 1 UAH/service; OP-155 and MANUAL:Трудовитрати виробів!1820 have nullable default minutes.
- pg_restore --list successfully read the pre-pricing archive. No restore drill was performed.

## Scoped audit and residual work

| Finding | Status / owner / verification |
| --- | --- |
| AF-PR-001: old division and hard-coded schedule differ from the approved source | Fixed in new Catalog/Pricing engine; source parity tests/report. Existing consumer algorithm is intentionally not redirected before TS-005. |
| AF-PR-002: mutable costs/settings cannot reproduce a prior calculation | Initial immutable versions and effective reads implemented in Catalog/Reference; PostgreSQL version-selection test. Subsequent edit/approval commands remain mandatory before production cutover. |
| AF-PR-003: OtherCost had no costing owner | Reference version/code/unit ownership implemented; FUR-032 contributes its versioned unit amount; actual parity and Domain tests. |
| AF-PR-004: price-history FK can turn a reference deletion into an uncontrolled failure | Host usage bridge checks Reference history through its Application contract; retained-history usage projection exercised against PostgreSQL. Existing delete handlers retain their controlled conflict path. |
| AF-PR-005: imported review flags must not become publication approval | Strict query keeps review blockers; arithmetic-only job is non-publishable and writes no approvals. |
| AF-PR-006: lossy numeric persistence could undermine parity | Domain rejects settings/reference values that cannot fit numeric(20,8); source values verified without rounding. |

No full solution suite, browser/E2E, production deployment or remote database write was performed. Existing NU1903 for Microsoft.OpenApi.YamlReader 2.7.5 is unrelated and was not upgraded.

