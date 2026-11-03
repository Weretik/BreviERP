# REF-005 — Product sections («Типи і розділи») from the Calculator sheet

**Status:** backend implemented and verified 2026-11-03; Admin page in brevi-web; data transfer pending. **Date:** 2026-11-03. **Module:** Reference, schema `reference`, no new module.

Stage 1 of the products transfer (plan: «Brevi: перенос «Товари» — анализ и этапы», 2026-11-03). Products and site categories need the sheet's product sections first. The product owner decided on 2026-11-03: «Товари» keep the product core, prices and composition; texts, SEO and site categories on two languages move to a separate Content module later; sections («Розділи») become the public site categories in that module.

## Source

«Типи і розділи», export of 2026-11-03: columns A–D, 8 rows (`Тип товару`, `Розділ`, `Активний`, `Коментар`): five sewing sections (Одяг ЛІТО, Одяг ЛІТО КМФ, Одяг кухаря/пекаря, Одяг ЗИМА, Одяг ЗИМА КМФ) and three PPE sections (ЗІЗ, Одяг утеплений, Одяг неутеплений). Columns F–L hold drop-down lists for other tabs (product type, category, «Категорія для розрахунку», «Тип складової»); they belong to the products stages, not to this table.

The tab «ЗІЗ» uses its own 11 categories in «Розділ / категорія» («Захист рук», «Робоче взуття літнє»…); only «Одяг утеплений» is on «Типи і розділи».

## Rules

- **R-01:** A section has a product type (Sewing or Ppe), a unique name, an active flag, a comment and a display order.
- **R-02:** The import matches sections by name, ignoring case and punctuation; row numbers become the display order. A preview (default) saves nothing. Applying saves nothing when any row has an error. `deleteMissing` deletes sections not in the sheet. Repeating the same file changes nothing.
- **R-03:** The transfer takes the 8 rows of «Типи і розділи» and adds every category used on «ЗІЗ» that is missing there as an active PPE section, so every PPE product can get a section.
- **R-04:** Products refer to a section by ID (stage 2), so renaming a section keeps the link.

## Scenarios

- **SC-01 / R-02:** Given a stored «Одяг ЛІТО» and an old section, when the sheet is applied with deletion, then «Одяг ЛІТО» is matched by name, the new section created with the next ID and its row order, and the old one deleted.
- **SC-02 / R-02:** Given a preview or a row with an unknown type, when the import runs, then nothing is saved.
- **SC-03 / R-01:** An unknown type, an empty or too long name are rejected; a file with repeated names is rejected as a whole.

## Design

- Domain: `ProductSections/` (`ProductSection`, `ProductSectionDetails`, `ProductSectionType`).
- Application: `Features/ProductSection/` (CRUD, list, `Import/`, FluentValidation). IDs are assigned by Brevi (next free).
- API: `api/reference/product-sections` (+ `/import`); reads anonymous, writes `CanManageProducts`. Contract: [product-sections.openapi.yaml](../../../contracts/reference/product-sections.openapi.yaml).
- Data: migration `AddProductSections` creates `reference.ProductSections` with a unique name index. Additive; rollback is the migration `Down`.
- Deleting a section does not check products yet: stage 2 adds the product link and the check.

## Tasks

- **TS-01 (done):** domain, migration, CRUD, list, import. Tests: `tests/UnitTests/Reference/ProductSectionTests.cs`.
- **TS-02:** Admin page «Типи і розділи» in brevi-web (reference list pattern).
- **TS-03:** One-time transfer to production: preview, owner's «да», apply.

## Verification 2026-11-03

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~ProductSectionTests"`: 6 passed. `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 7 passed.
- `dotnet ef migrations has-pending-model-changes` (ReferenceDbContext, Host.Seed startup, Production): no changes.
