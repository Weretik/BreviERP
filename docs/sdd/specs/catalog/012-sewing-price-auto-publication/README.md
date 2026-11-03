# CAT-012 — Automatic Sewing price publication

**Status:** backend implemented 2026-11-03. **Date:** 2026-11-03. **Module:** Catalog (publication), Reference (change signal), hosts (Host.Api filter, Host.Seed sync step). No migrations, no new tables.

After [CAT-011](../011-product-list-prices/README.md) a changed fabric, accessory, operation or setting left the published price old until someone pressed «Перерахувати всі», and the Admin showed two prices («опубліковано» and «зараз») with a «застаріли» marker. The product owner found this confusing and chose automatic publication: every saved change of a pricing input publishes the new prices of the affected models at once; a nightly step of `brevierp-sync` is the safety net. The button, its endpoint and the stale marker in the Admin go away; the price history stays.

## Scope

- In: publishing changed Sewing prices after any API request that saved a pricing input; the nightly `sewing-prices` step; removal of `recalculateSewingPrices` (`POST /api/v1/pricing/sewing/recalculation`).
- Out: the per-Article calculation and publication operations of the sewing card (unchanged, still in the contract); PPE prices (live, unchanged); the Admin changes (brevi-web).

## API

`recalculateSewingPrices` is removed from [sewing-pricing.openapi.yaml](../../../contracts/catalog/sewing-pricing.openapi.yaml). `getProductOverviewPrices` was later replaced by the paged list of [CAT-013](../013-product-list-page/README.md), which reads only published prices.

## Rules

- **R-01:** A pricing input change is a save that adds a Reference pricing input version (a new or changed price, minutes, unit or activity of a fabric, accessory or operation, or another cost) or deletes one of those rows: it affects every Sewing model. A save of a model's Articles, material lines or operation lines affects that model. A new sewing settings version (from «Параметри ціноутворення») affects every model. A rename or another change that adds no version affects nothing.
- **R-02:** After an API action that made such a change, the affected models are published: every active Article whose live price is calculable and was never published or differs from the published one gets a saved calculation and a publication, one transaction per model. The actor is the signed-in user, else `auto`.
- **R-03:** Publication runs in its own DI scope after the action, whatever its result. Its failure is logged and does not change the response: the saved change stays and the nightly step publishes it.
- **R-04:** If a model's inputs change during publication so that an Article is no longer calculable, that model is skipped (nothing of it is saved) and the run continues.
- **R-05:** An Article that cannot be calculated keeps its published price, if any.
- **R-06:** The nightly `brevierp-sync` step `sewing-prices` (actor `nightly-sync`) publishes every model under R-02, catching changes saved outside the API (jobs, scripts) and failed automatic runs. On a normal night it publishes nothing.

## Design

- BuildingBlocks.Application `Pricing/PricingInputChanges.cs`: scoped record of changed models (or «all»).
- Reference.Infrastructure `Pricing/InputVersions/Changes/ReferencePricingInputChangeInterceptor.cs` and Catalog.Infrastructure `Pricing/AutoPublication/SewingPricingInputChangeInterceptor.cs`: EF save interceptors that mark it after a successful save; registered with the DbContexts.
- Catalog.Application `Features/Pricing/Sewing/AutoPublication/`: `SewingPricePublisher` (R-02, R-04, R-05) and `SewingPriceStatusReader` (live and published price per Article, also used by the «Товари» prices read).
- Host.Api `Pricing/SewingPriceAutoPublicationFilter.cs`: global MVC action filter (R-02, R-03).
- Host.Seed `Jobs/Sync/SewingPricesSyncTask.cs` (R-06), runbook [sync job](../../../operations/jobs/sync-job.md#sewing-prices).

Cost on the Calculator data (124 models, 248 Articles, local PostgreSQL): checking every model about 2 s; publishing every model about 5 s; the full sheet import now takes about 14 s instead of 9 s because it publishes all prices.

## Acceptance

- **AC-01:** After the sheet import every calculable model is Ready with its prices published by the importing user; a repeat import publishes nothing (R-01, R-02).
- **AC-02:** Raising a fabric price publishes the new, higher price at once; the old publication stays in history; a rename publishes nothing (R-01, R-02).
- **AC-03:** A new seamstress salary in «Параметри ціноутворення» republishes every model (R-01).
- **AC-04:** A fabric price saved outside the API leaves the model Stale until the `sewing-prices` sync step publishes it (R-06).

## Tasks

- **TS-01 (done):** backend, contract, docs, tests: `tests/IntegrationTests/CatalogSheetImportStoreTests.cs` (`Sewing_prices_are_published_automatically_after_an_import_and_after_a_fabric_price_change`), `tests/IntegrationTests/CatalogSheetImportRealFileTests.cs` (all 248 Article prices published), `tests/IntegrationTests/SyncJobCommandTests.cs`.
- **TS-02:** brevi-web: remove «Перерахувати всі», its dialog, the stale marker and the manual «Розрахувати / Опублікувати» of the sewing card; show one price.
- **TS-03:** after deploy the first nightly run (or the first change) publishes the current production prices; nothing to run by hand.

## Verification 2026-11-03

- `dotnet test tests/IntegrationTests/IntegrationTests.csproj` (local PostgreSQL 16, with the Calculator transfer files): 266 passed, 108 skipped (need other services), 0 failed.
- `dotnet test tests/UnitTests/UnitTests.csproj`: 578 passed; `tests/ArchitectureTests`: 7 passed.
