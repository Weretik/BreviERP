# CAT-018 — Product photos, old-site photo import and the active switch

**Status:** implemented 2026-11-04. **Date:** 2026-11-04. **Module:** Catalog (with Media for the library side).

The owner asked (2026-11-04) to bring the photos of the old OpenCart site (brevi.com.ua) into the Media library, link
them to Brevi products so quotes (КП) show the main photo, and transfer the old sewing models Brevi does not have yet
as switched-off models. The product card gets a tab «Фото», the «Товари» list a thumbnail of the main photo, and the
list an «Активний» switch and filter, so staff see which products work and may later be shown on the public site.

## Decisions (owner, 2026-11-04)

- Only sewing is transferred from the old site; old PPE without a Brevi product is skipped.
- Missing sewing models are created **switched off**, without articles, composition or price; the existing sections suffice.
- Old photos go to the library folder «Старий сайт» with the old subfolders (the root `catalog` is dropped).
- Admin adds, removes and uploads product photos; the Director reorders and picks the main photo (the Director also holds Admin, so both are the `RequireAdminRole` policy); a Manager only views.
- The «Товари» list shows the main photo and the active state, with a switch and a filter.

## Rules

- **R-01:** A product's photos have an order and one main photo. `GET /api/v1/products/{id}/photos` (CatalogRead) lists them with library URLs; a photo whose library file is gone is marked `missing`.
- **R-02:** `PUT /api/v1/products/{id}/photos` (Admin) saves the whole list in order: remaining photos keep their id, alt text and visibility; new ones take the next ids; at most 60, no repeats, every new id must exist in the library. `mainMediaFileId` picks the main photo; null keeps the current one, or makes the first one main.
- **R-03:** The «Товари» row has `mainPhotoUrl`: the preview (or the full photo) of the main visible photo, the photo quotes use (`QuoteCatalogReader`).
- **R-04:** `GET /api/v1/products/overview?active=true|false` shows only active or only switched-off rows; the page counts switched-off rows of the type in `inactive`. The switch saves through `PUT /api/v1/products/{id}/overview` (CanManageProducts).
- **R-05:** `POST /api/v1/products/old-site-photo-import` (Admin) takes the file built from the old-site export (`old-site-photo-import.json`). A product is found by any old-site SKU: sewing article sale SKUs or the PPE site article (the old «model» 758-759 gives SKUs 758 and 759). Without a match, `newModel` is looked up by name among sewing products, then created through «Нова модель» (next MOD code, section by name, season, switched off).
- **R-06:** Each photo is a library file with source key `old-site:<path>` (unique, `media.Files.SourceKey`). A known key is reused; otherwise the file is downloaded from `baseUrl` + escaped path (https only, JPG/PNG/WebP, the library size limit, 30 s) and uploaded with that key. A failed photo is listed with its reason and does not stop the others.
- **R-07:** New photos are added after the product's own photos in old-site order, main first. The old main photo becomes main only when the product had no photos. A repeat adds nothing twice.
- **R-08:** `dryRun=true` (up to 2000 products) reports found, to-create and not-found products, photos to add and files to download, and changes nothing. A real run takes at most 20 products; the admin page sends the file in parts and sums the reports.

## Scenarios

- **SC-01:** Dry run of the 534 old products: 248 found by SKU, 123 models to create, PPE without a match not found; nothing is written.
- **SC-02:** Real run in parts: the 123 models appear switched off, photos appear in «Фото → Старий сайт/…», products show the main photo in the list, the card and new quotes.
- **SC-03:** A second real run adds nothing and downloads nothing.
- **SC-04:** A Manager opens «Фото» and sees the photos without buttons; Admin/Director add, remove, upload, reorder and pick the main photo.

## Design

- Media: `MediaFile.SourceKey` (nullable, unique when set; migration `MediaFileSourceKey`), `UploadMediaFileCommand.SourceKey`, `GetMediaFilesBySourceKeysQuery`, `EnsureMediaFolderPathCommand`.
- Catalog: `Product.SetPhotoList`, `Features/Product/Photos` (get, set, SKU index), `Features/Product/OldSitePhotoImport`, port `IProductPhotoSource` implemented in Host.Api (`CatalogProductPhotoSource`: HttpClient + Media commands), `IProductMediaReader.GetFilesAsync`, `IProductSectionReader.ListAsync`.
- Contract: [product-photos.openapi.yaml](../../../contracts/catalog/product-photos.openapi.yaml), [product-overview.openapi.yaml](../../../contracts/catalog/product-overview.openapi.yaml) (`active`, `inactive`, `mainPhotoUrl`).

## Risks

- A later sheet import with `deleteMissing=true` deletes the transferred models, as they are not in the sheet.
- Cloud Run must reach brevi.com.ua; a blocked site shows as failed photos with the reason, and the run can be repeated.

## Tests

- Unit: `ProductPhotoListTests`, `OldSitePhotoImportTests`, `MediaImportTests`, `ProductOverviewListingTests.Active_filter_*`.
- Integration (PostgreSQL): `ProductPhotoPersistenceTests`, `MediaLibraryMigrationTests`.
