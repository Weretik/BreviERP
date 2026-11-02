# REF-001 — Suppliers from the Calculator and KeepinCRM

**Status:** in progress (supplier fields implemented; KeepinCRM synchronization waits for the source API details). **Date:** 2026-11-02. **Module:** Reference, schema `reference`, no new module.

Step 1 of moving the client's Calculator (Google Sheet + Calculator app) into Brevi: the «Постачальники» sheet becomes the Brevi supplier reference. Fabrics and accessories follow in a separate step and link to these suppliers.

## Source

Sheet «Постачальники» (export 2026-11-02): 43 rows, columns `ID KeepinCRM`, `Назва`, `Що закуповуємо`, `Контактна особа`, `Телефон`, `Email`, `Сайт`, `Теги`, `Використовувати в проєкті`, `Оновлено`. A script fills the sheet from KeepinCRM. Every fabric and accessory supplier in the sheet is one of these 43 names.

## Rules

- **R-01:** A supplier has the existing fields plus `Email`, `PurchaseCategories` («Що закуповуємо»), `Tags`, `IsActive` («Використовувати в проєкті», default true), `KeepinCrmId` (unique when present) and `KeepinSyncedAt`.
- **R-02:** For a supplier with `KeepinCrmId`, KeepinCRM owns name, site, contact person, phone, email and tags. Brevi rejects changes to them (400) and accepts notes, purchase categories and activity.
- **R-03:** Brevi always owns notes, purchase categories and activity; synchronization never overwrites them.
- **R-04:** A supplier without `KeepinCrmId` is managed manually, as before.
- **R-05 (planned):** Synchronization reads the KeepinCRM suppliers, updates linked suppliers by `KeepinCrmId`, links an existing manual supplier with the same normalized name, and creates the rest with a new Brevi ID. It runs daily as a Cloud Run job and on demand from the admin page. A KeepinCRM supplier that disappears is not deleted; it is reported.

## Scenarios

- **SC-01 / R-01, R-04:** Given a manual supplier, when staff save email, purchase categories and activity, then the list returns them.
- **SC-02 / R-02:** Given a KeepinCRM supplier, when staff change its name or phone, then the API returns 400 and nothing changes; when they change notes or activity, then the change is saved.
- **SC-03 / R-02, R-03:** Given a KeepinCRM supplier with Brevi notes and purchase categories, when a new KeepinCRM snapshot is applied, then contacts and tags change and Brevi fields stay.

## Design

- Domain: `Supplier.Update` rejects contact changes for linked suppliers; `UpdatePurchasing` changes Brevi-owned fields; `ApplyKeepinCrmSnapshot` / `CreateFromKeepinCrm` take a `KeepinCrmSupplierSnapshot` and truncate oversized external values to column limits.
- Application: create/update requests and validators gain `Email`, `PurchaseCategories`, `IsActive`; the list DTO returns all fields.
- Data: migration `AddSupplierKeepinCrmAndPurchasing` adds nullable columns, `IsActive` default true and a filtered unique index on `KeepinCrmId`. Rollback: the migration `Down` drops them; existing rows are unaffected.
- Contract: [suppliers.openapi.yaml](../../../contracts/reference/suppliers.openapi.yaml) 1.1.0, additive.

## Tasks

- **TS-01 (done):** fields, rules R-01–R-04, migration, contract. Tests: `tests/UnitTests/Reference/SupplierTests.cs`.
- **TS-02 (blocked on source API details):** KeepinCRM adapter, sync use case, `POST /api/reference/suppliers/keepin-sync`, Host.Seed mode and Cloud Run job, secret for the API key.
- **TS-03:** frontend columns, filters, editor rules and sync button (brevi-web `docs/specs/admin/references/001-suppliers`).
