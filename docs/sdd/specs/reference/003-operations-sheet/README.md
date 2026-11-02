# REF-003 — Operations from the Calculator sheet

**Status:** implemented and verified with unit tests 2026-11-02. **Date:** 2026-11-02. **Module:** Reference, schema `reference`, no new module.

Step 3 of moving the client's Calculator into Brevi after [REF-002 fabrics and accessories](../002-fabrics-accessories-sheet/README.md): the sheet «Операції» becomes the Brevi sewing operations reference. The product owner decided on 2026-11-02 that only the sheet data is current: old Brevi operations and groups are deleted, names are Ukrainian only, and the transfer runs once through an import endpoint with a preview.

## Source

Export of 2026-11-02, sheet «Операції»: 187 rows OP-001…OP-188, all active, columns `ID операції`, `Група деталей` (27 groups), `Вузол / елемент` (67), `Варіант виконання`, `Назва операції`, `Норма часу, хв` (empty only for OP-155 «Нестандартна операція»: the time is entered per product), `Вартість роботи швачки, грн` and `Вартість з урахуванням збирання, грн` (formulas: minutes × minute cost, then × (1 + assembly rate) from «Налаштування цін»), `Дата перевірки норми часу` (26 empty), `Активна`, `Примітки до джерела`.

Brevi had 34 groups and 151 operations without codes, with Russian names. No name matches the sheet.

«Трудовитрати виробів» (product labour lines) belongs to the products step and is out of scope.

## Rules

- **R-01:** An operation has `Code`, `Element`, `Variant`, `MinutesPerProduct`, `NormCheckedAt`, `IsActive` (default true) and `Notes` besides group, name and minutes. The code is unique when present; the name stays unique within a group.
- **R-02:** `Min` must be above 0 unless `MinutesPerProduct`; then it is stored as 0.
- **R-03:** Labour cost is not stored. Clients compute it from `Min` and the sewing pricing settings, so it follows setting changes.
- **R-04:** The import matches a row by code; the rest are created with the next ID. A group is matched by name ignoring case and punctuation; an unknown one is created.
- **R-05:** A preview (default) changes nothing and lists per row create, update (with changed fields), unchanged or error. Applying saves nothing when any row has an error. Repeating the same file changes nothing.
- **R-06:** With «delete missing», operations not in the sheet are deleted. An operation still used by a product or price history is marked not used instead and reported as kept in use, so existing products keep resolving. Groups left without operations are deleted.
- **R-07:** A group that still has operations cannot be deleted (409).

## Scenarios

- **SC-01 / R-04, R-06:** Given a coded operation, an unused old one and a used old one, when the sheet is applied with delete missing, then the coded one is updated, a new group and operation are created, the unused one and its empty group are deleted, and the used one becomes inactive.
- **SC-02 / R-05:** Given a preview, or a row without a norm, then nothing is saved and the error is shown on the row.
- **SC-03 / R-02:** Given «time per product», then the code is normalized and the stored norm is 0.

## Design

- Domain: `GarmentPartOperationDetails`; `GarmentPartOperation.Create(id, details)`, `ApplyDetails`, `NormalizeCode`.
- Application: `Features/GarmentPartOperation/Shared/` (fields interface, validator, details factory, code spec), `Features/GarmentPartOperation/Import/`; create/update/list carry the new fields; `GarmentPart/Delete` rejects a group with operations.
- API: `POST /api/reference/garment-part-operations/import` (CanManageProducts).
- Data: migration `AddOperationSheetFields` adds nullable columns, defaults `IsActive=true`, `MinutesPerProduct=false`, and a filtered unique `Code` index. Rollback: the migration `Down`. Seed data is regenerated from the sheet (27 groups, 187 operations).
- Contract: [operations](../../../contracts/reference/garment-part-operations.openapi.yaml) 1.1.0, additive; [garment parts](../../../contracts/reference/garment-parts.openapi.yaml) delete gains 409.

## Tasks

- **TS-01 (done):** fields, migration, create/update, list, seed.
- **TS-02 (done):** import use case and endpoint. Tests: `tests/UnitTests/Reference/GarmentPartOperationImportTests.cs`.
- **TS-03:** Admin page «Операції» in brevi-web (pattern B, cost columns from the sewing settings).
- **TS-04:** one-time transfer: preview, then apply after the owner's confirmation.

## Verification 2026-11-02

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~Reference|FullyQualifiedName~Pricing|FullyQualifiedName~Sewing"`: 198 passed. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed.
