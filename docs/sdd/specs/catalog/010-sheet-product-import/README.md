# CAT-010 — Product import from the Calculator sheet

**Status:** backend implemented 2026-11-03, not applied to production. **Date:** 2026-11-03. **Module:** Catalog, schema `catalog`, no new module, no migration.

Stage 5 of moving the client's Calculator into Brevi, after the Reference imports ([REF-002](../../reference/002-fabrics-accessories-sheet/README.md), [REF-003](../../reference/003-operations-sheet/README.md)) and [CAT-009](../009-product-create-pages/README.md). The product owner decided that only the sheet data is current: Brevi products that are not in the sheet are deleted, names are Ukrainian only, and the transfer runs once through an import endpoint with a preview. Production writes happen only after the owner reviews a preview.

## Scope

- In: sewing models of «Товари» with articles («Прайс на сайт»), materials («Комплектація виробів») and operations («Трудовитрати виробів»); PPE of «ЗІЗ» with supplier fields and the price version.
- Out: site texts, SEO, photos and categories (planned [Content module](../../content/001-site-content/README.md)); reading the sheet (the transfer script writes the JSON body).

## API

Contract: [product-overview.openapi.yaml](../../../contracts/catalog/product-overview.openapi.yaml).

| Operation | Endpoint | Policy |
| --- | --- | --- |
| `importSheetProducts` | `POST /api/v1/products/sheet-import` → 200 `SheetImportReport` | `CanManageProducts` + actor claim |

Body `{ dryRun, deleteMissing, sewing[], ppe[] }`; `dryRun` defaults to true. The report has counts per type (created, updated, unchanged), `toDelete`, blocking `problems`, non-blocking `warnings` and `applied`.

## Rules

- **R-01:** A row matches a stored product by type and code (sewing model ID ignoring case, PPE site article), else by the exact name in either name column among products of the same type. Two rows matching one product is a problem.
- **R-02:** A matched product keeps its ID and slug and gets the name (the Russian column a copy), code, «Товари» fields and, for PPE, the supplier fields. A sewing model gets its article list replaced (an article with the same SKU keeps its ID) and its whole composition replaced. A new product is created as in CAT-009 (ID largest + 1, slug from `ProductSlugCandidates`).
- **R-03:** Codes resolve through `IProductReferenceCodeReader`: FAB-…, FUR-…, OP-… by code; suppliers by trimmed name ignoring case, with the explicit alias «3C-Макркет» → «3C-Маркет»; sections by name and type. Unknown codes, sections or suppliers are problems.
- **R-04:** Problems (blocking): a missing code or name; a code, a name (across both types) or a sale SKU (sewing articles and PPE articles) repeated in the body; a name, model code or SKU kept by a stored product the import does not touch; a material or operation article not in the model's articles; an operation tied to an article; a value the domain rejects; a wrong check date; a delete restricted by saved calculations. Any problem means nothing is written, also with `dryRun: false`. All problems are reported in one pass.
- **R-05:** Warnings (not blocking): an unknown PPE rule (the product is imported without a rule), no supplier price (no price version), no supplier link.
- **R-06:** The same operation may appear more than once in a model. The same material twice in the same scope stays a domain conflict (none in the current sheet).
- **R-07:** A checked sewing model gets every line marked reviewed by «Google-таблиця» at Kyiv midnight of `verifiedOn` (now when the date is empty). The readiness rules require reviewed required materials and reviewed operations for «Готово до розрахунку»; an unchecked model's lines stay unreviewed.
- **R-08:** A PPE price version (source «Імпорт з таблиці», source date `verifiedOn` or today) is appended only when the supplier price, the rule or a given Insight price differs from the current version. No supplier price means no version; missing Insight prices keep the current ones.
- **R-09:** With `deleteMissing`, every product of either type that no row matches is deleted, with its PPE offer versions. A product or article with saved sewing calculations (price history, publications) cannot be deleted: that is a problem.
- **R-10:** A product whose stored state already equals its row is unchanged and not written.
- **R-11:** The write is one database transaction: deletes are saved first, then cards and articles, then compositions, then price versions. A database rejection returns 409 and nothing is saved.

## Foreign keys into `catalog.Products`

| Table | On delete |
| --- | --- |
| `ProductCatalogProfiles`, `SewingArticles`, `SewingMaterialLines`, `SewingOperationLines`, `ProductPhotos`, `ProductCategoryLinks`, `ProductInformationBlocks`, `ProductCharacteristicTables` (+ rows), `SewingProductDetails` (+ legacy fabrics, accessories, operations), `PpeProductDetails` | cascade |
| `PpePricingOfferVersions` | restrict: the import deletes them first (R-09) |
| `SewingCalculations` (product and article), `SewingPricePublications` → calculations | restrict: reported as a problem (R-09) |

Other modules keep catalog product IDs as plain values without foreign keys (Crm quote variants, introduction featured mappings); deleted IDs stay there as history.

## Design

- Application `Features/Product/SheetImport/`: command, handler, validator, `DTOs/` (rows and report), `Planning/` (`SheetImportPlanner`, `SheetBodyDuplicates`, `SheetReferenceResolver`, `SheetProductMatcher`, `SheetSewingDraftBuilder`, `SheetPpeDraftBuilder`, `SheetProductApplier`, `SheetProductFingerprint`, plan and draft records). Contracts: `Contracts/Reference/IProductReferenceCodeReader.cs`, `Contracts/Persistence/SheetImport/ISheetProductImportStore.cs`. Shared slug choices: `Overview/Shared/ProductSlugCandidates.cs`.
- Domain: `Product.ReplaceArticles` (keeps an article with the same SKU, removes the rest with their lines).
- Infrastructure: `Products/SheetImport/SheetProductImportStore.cs`; `PpeOfferVersionWriter` joins an open transaction.
- Host.Api: `CatalogProductReferenceCodeReader` through the Reference list queries.
- API: `ProductOverviewController.ImportSheet`, `Contracts/Products/Overview/ImportSheetProductsRequest.cs`.

## Acceptance

- **AC-01:** A preview of the client's file lists creates, updates, unchanged rows, deletes, problems and warnings and writes nothing (R-04, R-05).
- **AC-02:** With no problems and `dryRun: false`, everything is written in one transaction; repeating it changes nothing (R-10, R-11).
- **AC-03:** Unknown codes, duplicates and restricted deletes stop the write (R-03, R-04, R-09).

## Tasks

- **TS-01 (done):** endpoint, planner, store, contract. Tests: `tests/UnitTests/CatalogSheetImportTests.cs`, `CatalogSheetImportHandlerTests.cs`. No integration test: the store uses PostgreSQL SQL and the integration suite needs `BREVIERP_CATALOG_TEST_CONNECTION`.
- **TS-02:** owner adds or maps the missing rules «Одяг ЗІЗ», «Захист голови» (optional; otherwise imported without a rule); preview on production, then apply ([runbook](../../../operations/data/migrations-and-seeding.md#sheet-product-import)).

## Verification 2026-11-03

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~SheetImport|FullyQualifiedName~CatalogProductCreation|FullyQualifiedName~ContractTests"`: 131 passed.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed.
- The planner over the real `products-import.json` with all its codes known: one problem (duplicate name «Рукавички антивібраційні», codes 73 and 74) and 19 warnings. The owner renamed them to «… (73)» and «(74)»; the regenerated body has no repeated names.
