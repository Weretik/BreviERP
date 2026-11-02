# REF-002 — Fabrics and accessories from the Calculator sheet

**Status:** implemented and verified with unit tests 2026-11-02. **Date:** 2026-11-02. **Module:** Reference, schema `reference`, no new module.

Step 2 of moving the client's Calculator into Brevi after [REF-001 suppliers](../001-suppliers-keepincrm/README.md): the sheets «Тканини» and «Фурнітура» become the Brevi fabric and accessory references, loaded once through an import endpoint with a preview. The product owner decided on 2026-11-02 that the Admin needs no import window: the team sends the sheet rows once (preview, then apply).

## Source

Export of 2026-11-02:

- «Тканини»: 86 rows FAB-001…, columns A `ID тканини`, B `Назва тканини`, C `Постачальник` (10 empty), D `Склад`, E `Щільність, г/м²`, F `Ціна за 1 м, грн`, G `Дата оновлення ціни`, H `Активна`, I `Коментар`, J `Название ткани (RU)`, K `Состав (RU)`, L `Плотность (RU)` (copy of E, not stored), M `Короткий опис тканини для КП`, T `Загальний перелік кольорів` (a separate colour list, not a fabric field; later step), U unnamed notes (price per kg, width; merged into the comment). Two names repeat with different codes.
- «Фурнітура»: 43 rows FUR-001…, columns A `ID фурнітури`, B `Назва фурнітури`, C `Постачальник` (6 empty), D `Ціна, грн`, E `Одиниця виміру` (шт. 21, м 21, послуга 1), F `Дата оновлення ціни`, G `Активна`, H unnamed note.

Brevi already had 44 fabrics and 36 accessories without codes (older seed data, different names).

## Rules

- **R-01:** A fabric has `Code`, `NameRu`, `CompositionRu`, `IsActive` (default true), `Comment`, `ProposalDescription` and `PriceUpdatedAt` besides the existing fields. An accessory has `Code`, `Unit` (шт., м, послуга; default шт.), `IsActive`, `Comment` and `PriceUpdatedAt`.
- **R-02:** The code is unique when present; names may repeat, as in the sheet.
- **R-03:** Editing a price in Brevi sets `PriceUpdatedAt` to today (Kyiv). The import takes the sheet's date. Earlier prices stay in [pricing input versions](../../../features/reference/pricing-input-versions.md) for versioned records; no second history is kept.
- **R-04:** The import matches a row by code; an older row without a code is linked when its name matches ignoring case and punctuation; the rest are created with the next ID. Supplier names are matched like the KeepinCRM sync; an unknown name becomes a new manual supplier, an empty one «Не вказано».
- **R-05:** A preview (default) changes nothing and lists per row create, update (with changed fields), unchanged or error. Applying saves nothing when any row has an error. Repeating the same file changes nothing.
- **R-06:** With «deactivate missing», rows that are not in the sheet become inactive («Не використовується»). Nothing is deleted.

## Scenarios

- **SC-01 / R-04, R-06:** Given a coded fabric, a same-name legacy fabric and an unknown one, when the sheet is applied with deactivation, then they are updated, linked and created, a new supplier is added, and a missing fabric becomes inactive.
- **SC-02 / R-05:** Given the same data as stored, when a preview runs, then rows are unchanged/create and nothing is saved.
- **SC-03 / R-05:** Given an accessory row with an unknown unit, when the import is applied, then the row is an error and nothing is saved.
- **SC-04 / R-01, R-03:** Given units «м.» and «Послуга», when imported, then they are stored as `м` and `послуга` with the sheet's price date.

## Design

- Domain: `Fabric` / `GarmentAccessory` gain `ApplyDetails(FabricDetails | GarmentAccessoryDetails)`, `SetPrice(price, date)`, `NormalizeCode`; accessory `NormalizeUnit`.
- Application: `Features/Fabric/Import/`, `Features/GarmentAccessory/Import/`, shared `Features/Shared/SheetImport/` (result DTOs, name key, supplier resolver, diff) and `Features/Shared/Dates/KyivDate.cs`. Create/update requests gain the new optional fields; name uniqueness checks are replaced by code uniqueness.
- API: `POST /api/reference/fabrics/import`, `POST /api/reference/garment-accessories/import` (CanManageProducts). The caller sends the sheet rows as JSON; the one-time transfer used the client's .xlsx (86 fabrics, 43 accessories). The endpoints stay for a repeat transfer if the sheet is still used in parallel.
- Data: migration `AddFabricAccessorySheetFields` adds nullable columns, defaults `IsActive=true` and `Unit='шт.'`, replaces unique name indexes with plain ones and adds filtered unique `Code` indexes. Rollback: the migration `Down` (fails if duplicate names were saved meanwhile).
- The older `CatalogImport` job matches by code before name, so it does not duplicate imported rows.
- Contract: [fabrics](../../../contracts/reference/fabrics.openapi.yaml) and [garment accessories](../../../contracts/reference/garment-accessories.openapi.yaml) 1.1.0, additive; a missing supplier on update now returns 400 instead of 404, duplicate names are allowed.

## Tasks

- **TS-01 (done):** fields, migration, create/update, list. Tests: `tests/UnitTests/Reference/FabricPresentationMetadataTests.cs`.
- **TS-02 (done):** import use cases and endpoints. Tests: `tests/UnitTests/Reference/ReferenceSheetImportTests.cs`.
- **TS-03:** Admin pages in brevi-web (no import window) (spec `admin/references/013-fabrics-accessories-sheet`).

## Verification 2026-11-02

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Reference"`: 18 passed. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed.
