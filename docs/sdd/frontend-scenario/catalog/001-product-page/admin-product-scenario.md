# Frontend Admin Product Scenario

**Audience:** a developer or AI building the catalog admin frontend.
**Scope:** list, create, view, edit, and delete `Sewing` and `Ppe` products. The public product page is outside this scenario.

This document defines the visible sections and the manager's action sequence. It does not duplicate DTOs or the design system. Get exact field names, types, required status, response codes, and request examples from [Product OpenAPI](../../../contracts/catalog/product-catalog.openapi.yaml). Data rules are in the [overview](../../../specs/catalog/001-product-page/requirements/overview.md), [product content](../../../specs/catalog/001-product-page/requirements/product-content.md), [Sewing](../../../specs/catalog/001-product-page/requirements/sewing.md), [PPE](../../../specs/catalog/001-product-page/requirements/ppe.md), and [pricing](../../../specs/catalog/001-product-page/requirements/pricing.md). If this scenario and OpenAPI differ on HTTP behavior, follow OpenAPI and clarify the scenario before implementation.

## 1. Screens and Navigation

| Screen | What the manager sees | Main actions |
| --- | --- | --- |
| List | Search, type and category filters, sorting, pagination, and product rows | Create a product; open a product; change list options |
| Create | One form with shared and type-specific sections | Enter data, choose `Sewing` or `Ppe`, save, cancel |
| View | Full admin product detail with all data, reference names, and calculated values | Edit or delete the product |
| Edit | The same section structure, populated from full admin detail | Change data, including type; save the full state or cancel |

This SDD does not define frontend route names, column layout, tab appearance, or visual style. The sections below must be available regardless of the chosen layout.

## 2. Product List

1. Load `GET /api/v1/products`. Show the ID, Ukrainian name, type, main photo or empty-photo placeholder, categories from the available `categoryIds`, `minimumWholesalePrice`, and creation/update dates in each row. `slug` is available in the response but is not a search field. Displaying category names requires a category list; Product list returns only category IDs.
2. A row opens the full detail using `GET /api/v1/products/{id}`. Do not build the detail view from the abbreviated list object.
3. Send search as a single parameter: the backend searches for an exact ID when given one numeric token, or partial token matches in `uk`/`ru` names. Filters: one type (`Sewing`/`Ppe`) and one category; for a parent category, the backend includes descendants.
4. Sort by ID, name, creation date, or update date, in `asc`/`desc` order. Default to name ascending. Show pagination using `value` and `pagedInfo`: default `pageSize` is 20, allowed sizes are 10/20/50. Return to the first page when search or filters change.
5. `minimumWholesalePrice = 0` means the list price has not been calculated; do not present zero as a confirmed commercial price. Show detailed prices on the product detail page.

## 3. Create a Product

The form may be one screen or a sequence of steps, but it saves the product in one `POST /api/v1/products` request. The sections below define dependencies; moving between them does not create partial Product records.

### Section 1 — Identification

- The manager enters a positive, unique ID for the new product, Ukrainian and Russian names, and selects `Sewing` or `Ppe`. Both names are required, each must be unique, and neither may exceed 200 characters.
- Do not enter or send `slug`. The backend generates it from the Ukrainian name; show it after a successful save.
- After the type is selected, show only the corresponding Section 4. Send the API `type` field exactly as `Sewing` or `Ppe`.

### Section 2 — Categories and Photos

- The manager selects zero or more categories without duplicate IDs. Load options using `GET /api/reference/product-categories/admin` as described by the [dependency contract](../../../contracts/catalog/product-dependencies.openapi.yaml).
- The manager selects existing photos through `GET /api/catalog/media` or uploads a new one through `POST /api/catalog/media` (`multipart/form-data`). Link only media files in the `Ready` state to Product; the upload alone does not create a product link.
- Configure `alt`, visibility, order, and main-photo status for each selected photo. If there are photos, exactly one must be main. Show its preview using the ready URL; send `mediaFileId` and link fields in the request, not the URL. Do not add the same media file twice.

### Section 3 — Descriptive Content

- `Description`: Ukrainian and Russian text are required, up to 20,000 characters each. Use Markdown with the constructs allowed by the [content requirements](../../../specs/catalog/001-product-page/requirements/product-content.md); do not enter raw HTML, images, or Markdown tables. Render previews with a sanitized renderer.
- `Information`: an ordered list of separate localized “heading + plain text” blocks. Allow adding, editing, deleting, and reordering blocks.
- `Characteristics`: an ordered list of localized tables, each containing ordered “property + value” rows. Allow managing both tables and rows. Persist order in `sortOrder`, not only in visual placement.

### Section 4A — `Sewing` Only

- Enter `metersPerProduct`, a positive number of fabric meters per product.
- Select fabrics from the [Fabric reference](../../../contracts/reference/fabrics.openapi.yaml). Set `isPrimary` and `sortOrder` for each. Do not add a fabric twice; if exactly one is selected, it is primary; at most two may be primary, with the rest additional. Show primary fabrics first, ordered within each group by `sortOrder`.
- Select accessories from the [GarmentAccessory reference](../../../contracts/reference/garment-accessories.openapi.yaml). Set a positive per-product `quantity` and `sortOrder` for each, without duplicate IDs.
- Select operations from the [GarmentPartOperation reference](../../../contracts/reference/garment-part-operations.openapi.yaml), without duplicate IDs. Get duration from the reference data rather than entering it on Product. A product may be created as a draft without operations; `piecesPerShift` and dependent calculations may then be absent.
- Do not manually enter the three prices for each fabric or productivity. After saving, show them from the Product detail response if the backend calculated them. Before the first save, do not present a local estimate as a server price.

### Section 4B — `Ppe` Only

- Select exactly one supplier from the [Supplier reference](../../../contracts/reference/suppliers.openapi.yaml).
- Enter a positive `basePrice` manually.
- Choose `Reference` or `Custom` separately for retail and wholesale percentages. For `Reference`, select an [AdditionalReference](../../../contracts/reference/additional-references.openapi.yaml) entry with unit `%` and send its ID. For `Custom`, enter a non-negative percentage; no reference record is created.
- Do not enter retail or wholesale prices manually. After saving, show the calculated `retailPrice` and `wholesalePrice` from the response.

### Save

1. Check required fields, positive values, duplicates, presence of fields for only the selected type, and photo readiness. This is a client-side usability check; the backend remains authoritative.
2. Send one `POST /api/v1/products` with `CreateProductRequest`. The ID appears only in the create body. Allow only one active form submission; the contract does not define an `Idempotency-Key`.
3. On `201 Created`, use the returned full `ProductDetail` to show the product: the slug, photo URLs, reference names, and current calculations are already in the response. No additional `GET` for each selected ID is needed.

## 4. View a Product

Load `GET /api/v1/products/{id}` and show:

1. **Header:** ID, `uk`/`ru` names, type, generated slug, and dates.
2. **Gallery and categories:** all photos with URLs, `alt`, visibility, main-photo status, and order; categories with the details already available in `ProductDetail`.
3. **Content:** localized Description, Information, and Characteristics in their defined order. The interface language determines which of the two texts is shown; both languages remain available for editing.
4. **For `Sewing`:** meters per product, fabrics with primary flags and current reference prices, accessories with quantities and current prices, operations with current durations, `piecesPerShift`, prices for `1–10`, `11–39`, and `40+` for each fabric, and min/max by range. If a calculation is absent (`null`), show a “not calculated yet” state without inventing values. Do not round fractional `piecesPerShift` values from the API.
5. **For `Ppe`:** supplier, base price, source and value for each percentage, and calculated retail and wholesale prices.

Reference fields in the detail response are current at read time. Do not store their names, prices, or minutes as Product-owned fields, and do not expect a historical snapshot.

## 5. Edit and Change Type

1. Populate the form from full `ProductDetail`, not the list row. Show the ID as read-only: `PUT /api/v1/products/{id}` gets it from the route and does not accept it in the body.
2. Edit the same sections as in creation. When switching between `Sewing` and `Ppe`, warn that saving will delete the previous type's data. Clear irrelevant fields before submitting and require the new type's required section.
3. Send the **entire** current editable state in one `ReplaceProductRequest`: shared fields, photos, categories, content, and data for one type. Do not use a partial PATCH or omit unchanged collections just because the manager did not open them.
4. On `200 OK`, replace the local detail with the returned `ProductDetail`; it contains the new slug and recalculated prices. If the manager cancels editing, do not send `PUT`.

`ProductDetail` is a read projection, not a ready-made `ReplaceProductRequest`. Map the data as follows when initializing the form and before `PUT`:

| In `ProductDetail` | In `ReplaceProductRequest` |
| --- | --- |
| `categories[].id` | `categoryIds[]` |
| `photos[]` with URLs | `photos[]` with `mediaFileId`, `alt`, `isVisible`, `isMain`, and `sortOrder`; do not send URLs |
| `sewing.metersPerProduct` | `metersPerProduct` in the selected `Sewing` request |
| `sewing.fabrics[]`, `sewing.accessories[]` with reference names and prices | `fabrics[]`, `accessories[]` with IDs, primary flag/quantity, and order; do not send reference names or prices |
| `sewing.operations[].id` | `operationIds[]`; do not send names or minutes |
| `ppe.supplier.id` | `supplierId` |
| `ppe.basePrice` | `basePrice` in the selected `Ppe` request |
| `ppe.retailPercent` / `ppe.wholesalePercent` in `Reference` mode with `reference.id` | Corresponding object `{ source: "Reference", additionalReferenceId }` |
| The same percentages in `Custom` mode | Corresponding object `{ source: "Custom", customPercent }` |
| `slug`, `piecesPerShift`, `prices`, `retailPrice`, `wholesalePrice` | Do not send: these values are generated by the backend |

When changing the percentage mode, clear the previous mode's value so that only one of `additionalReferenceId` or `customPercent` is sent.

## 6. Delete

Show a confirmation for permanent Product deletion. After `DELETE /api/v1/products/{id}` returns `204 No Content`, return to the list and remove the product. Product details and links are deleted; media files in Media remain. Do not delete media files as part of this action.

## 7. UI States and Errors

| Situation | Frontend behavior |
| --- | --- |
| Loading list, detail, or reference data | Show a loading state; do not treat data that has not arrived as an empty list. |
| Empty list or empty reference response | Explain that there are no products or selectable options. For required reference data, prevent saving an invalid selection. |
| `400 Bad Request` | Show `ValidationError[]`, map `identifier` to the relevant field or section when possible, and preserve entered data. |
| `409 Conflict` | Show the API message; typical causes include duplicate ID/name/slug or a related entity conflict. Keep the form available for correction. |
| `404 Not Found` for detail/update/delete | Explain that the product is no longer available and allow returning to the list. |
| Network or server error | Preserve entered data and allow retry; do not show success without a confirmed API response. |

Get HTTP error formats from Product OpenAPI and the [human-readable contract](../../../specs/catalog/001-product-page/contracts/api-contract.md). `404` does not require a response body; do not invent an error envelope for it.

## 8. Actions and API Matrix

| Manager action | API / source |
| --- | --- |
| View, search, and filter products | `GET /api/v1/products` |
| Create a product | `POST /api/v1/products` |
| Open full detail | `GET /api/v1/products/{id}` |
| Save edits or a type change | `PUT /api/v1/products/{id}` |
| Delete a product | `DELETE /api/v1/products/{id}` |
| Select categories and fetch/upload photos | [Product form dependencies](../../../contracts/catalog/product-dependencies.openapi.yaml) |
| Select fabrics, accessories, operations, supplier, and reference percentages | Corresponding [Reference OpenAPI](../../../contracts/reference/) |

## 9. Verifiable Frontend Acceptance Scenarios

- The manager creates a `Sewing` product with one primary Fabric, accessories, and operations; after `201`, the manager sees all selected data, `piecesPerShift`, and available prices from the response. A `Sewing` product without operations is saved as a draft, and missing calculations are not replaced with zero in detail.
- The manager creates a `Ppe` product with a supplier, base price, one `Reference` percentage, and one `Custom` percentage; after `201`, both sources and the calculated retail and wholesale prices are shown.
- In the list, the manager searches by ID or name and changes type/category, sorting, and page size; the list shows only the main photo and `minimumWholesalePrice` from the list response.
- Editing preserves unchanged collections. Switching from `Sewing` to `Ppe` warns about losing Sewing data and sends the complete PPE state without Sewing fields.
- Invalid requests and conflicts show an error without losing form input; clicking Save again during an active request does not create a second request.
- After deleting a Product, it is absent from the list and previously attached MediaFiles are not deleted automatically.
