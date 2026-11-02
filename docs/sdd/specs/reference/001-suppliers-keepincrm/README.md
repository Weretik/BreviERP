# REF-001 — Suppliers from the Calculator and KeepinCRM

**Status:** implemented and verified with unit tests 2026-11-02; live KeepinCRM run pending the `keepincrm-api-key` secret. **Date:** 2026-11-02. **Module:** Reference, schema `reference`, no new module.

Step 1 of moving the client's Calculator (Google Sheet + Calculator app) into Brevi: the «Постачальники» sheet becomes the Brevi supplier reference. Fabrics and accessories follow in a separate step and link to these suppliers.

## Source

Sheet «Постачальники» (export 2026-11-02): 43 rows, columns `ID KeepinCRM`, `Назва`, `Що закуповуємо`, `Контактна особа`, `Телефон`, `Email`, `Сайт`, `Теги`, `Використовувати в проєкті`, `Оновлено`. The sheet's Apps Script «Оновити постачальників» fills every column except «Використовувати в проєкті» from KeepinCRM `GET https://api.keepincrm.com/v1/suppliers?page=N` (header `X-Auth-Token`, 25 per page; custom fields `shcho_zakupovuiemo_323`, `sait_324`, `kontaktnaya_osoba_297`; `phones`, `emails`, `tag_names`). Every fabric and accessory supplier in the sheet is one of these 43 names. Brevi repeats the script's field rules and replaces it.

## Rules

- **R-01:** A supplier has the existing fields plus `Email`, `PurchaseCategories` («Що закуповуємо»), `Tags`, `IsActive` («Використовувати в проєкті», default true), `KeepinCrmId` (unique when present) and `KeepinSyncedAt`.
- **R-02:** For a supplier with `KeepinCrmId`, KeepinCRM owns name, purchase categories, site, contact person, phones, email and tags (as in the sheet). The full update returns 400 for such a supplier.
- **R-03:** Brevi always owns activity and notes, edited through `updateSupplierBreviFields` for any supplier; synchronization never overwrites them.
- **R-04:** A supplier without `KeepinCrmId` is managed manually, as before; it can be added by hand at any time.
- **R-05:** Synchronization reads all KeepinCRM suppliers, refreshes linked suppliers by `KeepinCrmId`, links an unlinked supplier whose name matches ignoring case, punctuation and legal form words, and creates the rest with the next Brevi ID. A name already used by another supplier gets ` (KeepinCRM <id>)`. Nothing is deleted; linked suppliers KeepinCRM no longer returns are reported. An error or an empty answer changes nothing. It runs daily as the `brevierp-keepin-suppliers` Cloud Run job and on demand from the admin page.

## Scenarios

- **SC-01 / R-01, R-04:** Given a manual supplier, when staff save email, purchase categories and activity, then the list returns them.
- **SC-02 / R-02, R-03:** Given a KeepinCRM supplier, when staff send a full update, then the API returns 400 and nothing changes; when they change notes or activity, then the change is saved.
- **SC-04 / R-05:** Given a linked, a same-name manual and an unknown KeepinCRM supplier, when the sync runs, then they are updated, linked and created, activity and notes stay, and one save is made; an empty answer saves nothing.
- **SC-03 / R-02, R-03:** Given a KeepinCRM supplier with Brevi notes and purchase categories, when a new KeepinCRM snapshot is applied, then contacts and tags change and Brevi fields stay.

## Design

- Domain: `Supplier.Update` rejects card changes for linked suppliers; `SetActive` / `SetNotes` change Brevi-owned fields; `ApplyKeepinCrmSnapshot` / `CreateFromKeepinCrm` take a `KeepinCrmSupplierSnapshot` and truncate oversized external values to column limits.
- Application: `Features/Supplier/KeepinCrmSync/` (sync service, name key, command), `Features/Supplier/UpdateBreviFields/`, port `Contracts/KeepinCrm/IKeepinCrmSupplierSource`. Create/update requests gain `Email`, `PurchaseCategories`, `IsActive`.
- Infrastructure: `KeepinCrm/` (options, HTTP client, parser of the script's field rules), `DependencyInjection/ReferenceKeepinCrmExtensions.cs` used by Host.Api and Host.Seed.
- Host.Seed: `Jobs/KeepinSuppliersSyncCommandRunner.cs`, mode `--keepin-suppliers-sync=true`; deploy workflow job `brevierp-keepin-suppliers`; [runbook](../../../operations/jobs/keepin-suppliers.md).
- Data: migration `AddSupplierKeepinCrmFields` adds nullable columns, `IsActive` default true, a filtered unique index on `KeepinCrmId`, and widens `PhoneNumber` to 100 for several KeepinCRM phones. Rollback: the migration `Down`; existing rows are unaffected.
- Contract: [suppliers.openapi.yaml](../../../contracts/reference/suppliers.openapi.yaml) 1.2.0, additive except that `updateSupplier` now returns 400 for KeepinCRM suppliers (none exist before the first sync).

## Tasks

- **TS-01 (done):** fields, rules R-01–R-04, migration, contract. Tests: `tests/UnitTests/Reference/SupplierTests.cs`.
- **TS-02 (done, live run pending secret):** KeepinCRM adapter, sync use case, `POST /api/reference/suppliers/keepin-sync`, `PUT /api/reference/suppliers/{id}/brevi-fields`, Host.Seed mode and Cloud Run job. Tests: `tests/UnitTests/Reference/KeepinCrmSupplierParserTests.cs`, `KeepinCrmSupplierSyncTests.cs`.

## Verification 2026-11-02

`dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Reference`: 14 passed. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed. KeepinCRM itself is not reachable from the development environment; the parser is checked against the response shapes the client's script handles.
- **TS-03:** frontend columns, filters, editor rules and sync button (brevi-web `docs/specs/admin/references/001-suppliers`).
