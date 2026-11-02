# REF-004 — Pricing parameters from the Calculator sheet

**Status:** backend implemented and verified 2026-11-02; Admin page and pricing link pending. **Date:** 2026-11-02. **Module:** Reference, schema `reference`, no new module.

Step after [REF-002](../002-fabrics-accessories-sheet/README.md). The product owner decided on 2026-11-02: the Admin section «Додаткові довідники» becomes a full equivalent of the sheet «Налаштування цін», renamed «Параметри ціноутворення»; only the sheet's data counts, the old rows are deleted; Ukrainian only; this page becomes the single place for these numbers.

## Source

«Налаштування цін», export of 2026-11-02:

- Left table A–H, 40 rows: `Група параметрів`, `Код параметра`, `Назва параметра`, `Значення`, `Одиниця виміру`, `Тип значення` (Ручне / Розрахункове), `Дата оновлення`, `Коментар`. One calculated row: `MINUTE_COST_SEAMSTRESS` = `D2/(D4*D5)`. Names repeat («Коефіцієнт ціни: до 10 шт.» in 8 groups), codes do not.
- Right table J–O, 10 rows: `Правило ціноутворення`, `Роздріб, %`, `Опт, %`, `Великий опт, %`, `Примітка для прайсу`, `До яких товарів належить`. J13 is an instruction, not a rule.

Brevi had 14 old «Додаткові довідники» rows (Russian names, lower-case keys, outdated salary and cutting rate).

## Rules

- **R-01:** A parameter has a unique upper-case code that never changes, group, name, value, unit, calculated flag, update date, comment and display order. Percentages are fractions, as in the sheet.
- **R-02:** A calculated parameter stores no value; Brevi computes it from a known formula. A manual parameter needs a value ≥ 0.
- **R-03:** A PPE markup rule has a unique name that never changes (PPE offers refer to it), three markups 0…1000 %, a price note and «до яких товарів належить».
- **R-04:** Editing a value sets the update date to today (Kyiv); the import takes the sheet's date.
- **R-05:** The import matches parameters by code and rules by name; row numbers become the display order. A preview (default) saves nothing. Applying saves nothing when any row has an error. `deleteMissing` deletes rows not in the sheet. Repeating the same file changes nothing.
- **R-06:** Deploying the migration loses no data: old rows stay until the import deletes them after the owner's confirmation.

## Scenarios

- **SC-01 / R-02:** Given salary 17 500, 20.5 days and 480 minutes, when parameters are listed, then the minute cost is 17 500 / (20.5 × 480).
- **SC-02 / R-05:** Given a stored parameter and an old row, when the sheet is applied with deletion, then the parameter is updated, the new one created, the old row deleted.
- **SC-03 / R-05:** Given a preview or a row without a value, when the import runs, then nothing is saved.
- **SC-04 / R-01, R-02:** A calculated row with a value, a calculated row without a formula, a negative value and a lower-case code with spaces are rejected.

## Design

- Domain: `PricingParameters/` (`PricingParameter`, `PricingParameterDetails`, `PricingParameterFormulas`), `PpeMarkupRules/` (`PpeMarkupRule`, `PpeMarkupRuleDetails`). `AdditionalReference` is renamed; the legacy namespace was removed after the operations work merged.
- Application: `Features/PricingParameter/`, `Features/PpeMarkupRule/` (CRUD, FluentValidation), `Features/PricingSettingsImport/` (one import for both tables). IDs are assigned by Brevi (next free).
- API: `api/reference/pricing-parameters` (+ `/import`) and `api/reference/ppe-markup-rules`; reads anonymous, writes `CanManageProducts`. Replaces `api/reference/additional-references` (breaking; the Admin is updated in the same step).
- Data: migration `PricingSettingsFromSheet` renames `AdditionalReferences` to `PricingParameters` (Key → Code, Description → Comment), widens columns, drops the unique name index, adds `Group` (old rows «Старі довідники»), `IsCalculated`, `UpdatedOn`, `SortOrder`, and creates `PpeMarkupRules`. The old JSON seeder is removed: data comes only from the sheet import. Rollback: the migration `Down` (drops `PpeMarkupRules`).
- Catalog keeps reading parameters as `AdditionalReferenceItem` for old PPE percent links.
- Sync to Catalog (TS-04): after every saved create, update, delete or applied import, Reference calls the Catalog-owned port `IPricingSettingsChangeNotifier`. Host.Api implements it (`CatalogPricingSettingsSync`): it reads both lists and sends Catalog `SyncPricingSettingsCommand`. Catalog maps the 13 sheet codes (`SewingSettingsCodes`) to `SewingCalculationSettings` and appends a new approved sewing settings version only when a value changed; each PPE markup rule appends a PPE rule version by name (source «Параметри ціноутворення») only when its markups changed. History stays: old versions are never edited. The Reference save is not undone if the sync fails; a missing code, invalid value or missing base sewing version is logged as a warning and that part is skipped. The «Операції» page and prices read the Catalog current settings, so they show the new numbers after the next read.

## Tasks

- **TS-01 (done):** domain, migration, CRUD, list with calculated value. Tests: `tests/UnitTests/Reference/PricingSettingsTests.cs`, `PricingReferenceQueryHandlerTests`, integration `PricingReferenceQueryIntegrationTests`.
- **TS-02 (done):** sheet import with preview. Tests: `PricingSettingsTests`.
- **TS-03 (done, brevi-web #20):** Admin page «Параметри ціноутворення» in brevi-web (two tabs, reference list pattern).
- **TS-04 (done):** Catalog sewing settings and PPE rule versions take their values from these tables. Tests: `tests/UnitTests/PricingSettingsSyncTests.cs`, notifier calls in `PricingSettingsTests`.
- **TS-05:** One-time transfer to production: preview, owner's «да», apply.

## Verification 2026-11-02

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~PricingSettingsTests|FullyQualifiedName~PricingReferenceQueryHandlerTests|FullyQualifiedName~ReferenceSheetImportTests"`: 9 passed. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed. Integration `PricingReferenceQueryIntegrationTests` on a local PostgreSQL 16: 4 passed.
- `dotnet ef migrations has-pending-model-changes` (ReferenceDbContext, Host.Seed startup, Production): no changes.
- Local PostgreSQL: migration up keeps two old rows as «Старі довідники»; down and up again keep them. Host.Api on that database: the sheet preview reports 40 + 10 to create and the 2 old rows to delete; apply saves them; a second apply reports 40 + 10 unchanged; the list returns the minute cost 1.7785 грн/хв.

## Verification TS-04, 2026-11-02

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~PricingSettings|FullyQualifiedName~Reference|FullyQualifiedName~SewingPricing|FullyQualifiedName~PpePricing"`: 99 passed. ArchitectureTests: 7 passed.
- Local Host.Api with one old sewing version (salary 16 500): applying the sheet import appended sewing version 2 (17 500, 20,5 days, cutting 21,74 %) and 10 PPE rule versions with source «Параметри ціноутворення»; a second apply appended nothing; editing the salary to 18 000 on the page appended version 3, and `GET api/v1/pricing/sewing/settings` returned it.
