# MED-001 — «Фото»: photo library with folders (Media module)

**Status:** stage 1 (backend) implemented 2026-11-03. **Date:** 2026-11-03. **Module:** new module `Media`, schema `media`, own migration history `media.__EFMigrationsHistory` ([module rules](../../../standards/module-rules.md)).

## Why

Photos lived inside Catalog (`catalog.MediaFiles`, `/api/catalog/media`) and in the Admin under «Довідники → Медіа/Фото»: one flat list, no folders, search only in the browser, no paging, originals loaded as previews, an upload record saved before the CDN upload, and `[AllowAnonymous]` on upload and delete. Products, the future site content ([CNT-001](../../content/001-site-content/README.md)) and quotes all need the same photos, so the product owner chose on 2026-11-03 to move them into a separate module (option A of the audit; B = folders inside Catalog, C = inside the future Content module).

## Rules

- R-01: Folders form a tree of up to 8 levels. Names are trimmed, single-spaced, up to 120 characters, without `/` or `\`, unique among siblings ignoring case.
- R-02: Folders exist only in the database. Moving or renaming a folder or a photo never moves the CDN file, so its URL stays valid.
- R-03: Only an empty folder (no photos, no subfolders) can be deleted. A folder cannot move into itself or its subfolder.
- R-04: Upload accepts JPEG, PNG or WebP whose bytes match the type, up to `MediaUpload:MaxFileSizeBytes` (15 MiB). The file goes to the CDN first; the row is saved only after the CDN accepted it; if saving fails, the CDN file is removed.
- R-05: Search runs on the server by part of the name (case-insensitive), in the whole library or in a folder with or without subfolders, newest first, up to 100 per page.
- R-06: A photo used by another module (today Catalog product photos) cannot be deleted; the 409 message names the products. The detail read lists the usages.
- R-07: Access is Manager or Admin (policy `CanManageProducts`) for every operation, including reads.
- R-08: Other modules keep only the photo id. Catalog reads URLs through `IProductMediaReader`; Media asks usages through `IMediaUsageReader`. Both adapters live in Host.Api and use Mediator queries; no module references another, no cross-schema foreign keys.

## Data and rollout

- Tables: `media.Folders` (self-reference `ParentId`), `media.Files` (`FolderId`, name, alt, CDN key and URL, type, size, width/height for stage 3). See [Media schema](../../../architecture/data-model/media.md).
- Migration `InitialMedia` copies uploaded rows (`Status = 2`) of `catalog."MediaFiles"` with the same ids into a new top-level folder «Товари» and moves the id sequence past them. Unfinished uploads (`Status = 1`) have no CDN file and are not copied. A new database without Catalog gets an empty library.
- Catalog migration `MoveMediaFilesToMediaModule` only removes the table from the Catalog model; `catalog."MediaFiles"` stays in the database as a fallback copy. Drop it in a later migration after production is checked.
- Configuration `CatalogMediaUpload` is replaced by `MediaUpload` (`MaxFileSizeBytes`, `BaseFolder = /library/original`). `AdmToolsStorage` and its secrets keep their names.
- Rollback: deploy the previous image; the old table still holds every photo. New uploads made after the switch exist only in `media.Files`.

## Contract

[media-library.openapi.yaml](../../../contracts/media/media-library.openapi.yaml): `getMediaFolders`, `createMediaFolder`, `updateMediaFolder`, `deleteMediaFolder`, `searchMediaFiles`, `getMediaFile`, `uploadMediaFile`, `updateMediaFile`, `moveMediaFiles`, `deleteMediaFile`. The old `getCatalogMedia`, `uploadCatalogMedia` and `deleteCatalogMedia` are removed; the Admin switches in the same release.

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | Backend module, folders, upload/search/move/delete, migration of old photos, closed access | implemented 2026-11-03 |
| 2 | Admin section «Фото»: folder tree, photo grid, search, multi-upload, side panel | next |
| 3 | Thumbnails (about 400 px WebP) and width/height on upload, backfill for old photos | planned |
| 4 | «Вибрати фото» picker in the product card, after the current «Товари» work | planned |

## Verification — 2026-11-03

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Media` — folder tree rules, unique names, move into subfolder, empty-folder delete, CDN-first upload, missing folder, search in a branch, move keeps URL, used photo not deleted, fake image and size limit. Full UnitTests: 592 passed.
- Architecture: `MediaDependencyTests` (layer references, own schema and history, Catalog does not reference Media) and container restore coverage: 10 passed.
- Integration on local PostgreSQL 16: `MediaLibraryMigrationTests` (ids 41 and 43 copied into «Товари», unfinished upload skipped, next id 44, old table kept; empty database) and the changed Catalog tests: passed.
- Local smoke on a disposable database after `Host.Seed` migrations: folder create, duplicate name 409, move into subfolder 409, invalid name 400, fake image 400, non-empty folder delete 409, `/api/v1/products` list 200.
