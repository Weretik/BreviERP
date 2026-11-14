# Photo library («Фото»)

Folders and subfolders of photos on the Brevi CDN: upload, search by name or folder, rename, move and delete. Every photo gets its size and a WebP preview (480 px) on upload; the `brevierp-sync` step `media-thumbnails` (Mon–Fri 09:00) fills them for older photos. Products keep the photo id and read the URL through the host. [SDD](../../specs/media/001-media-library/README.md), [contract](../../contracts/media/media-library.openapi.yaml), [tables](../../architecture/data-model/media.md).

## Code structure

```text
src/Modules/Media/
  Media.Domain/Media.Domain/
    Folders/           # MediaFolder, MediaFolderTree (depth, branch, move checks)
    Files/MediaFile.cs # photo row created after the CDN upload; SetImage stores size and preview
    MediaNames.cs      # name normalization shared by folders and photos
  Media.Application/Media.Application/
    Abstractions/      # IMediaDbContext, IMediaStorage (CDN), IMediaImageProcessor, IMediaUsageReader, MediaUploadSettings
    Contracts/         # request/response DTOs
    Folders/           # GetFolders, CreateFolder, UpdateFolder (rename/move), DeleteFolder; shared messages and validator
    Files/             # SearchFiles, GetFile (+usages), UploadFile (signature check), UpdateFile, MoveFiles, DeleteFile, GetFilesByIds
    Files/Images/      # MediaImagePreparer: size + preview upload, shared by upload and backfill
    Files/BackfillImages/ # MediaImageBackfill: brevierp-sync previews for unprocessed photos
  Media.Infrastructure/Media.Infrastructure/
    Persistence/       # MediaDbContext (schema media), configurations, migrations
    Storage/           # AdmToolsMediaStorage (CDN adapter, download by public URL), options
    Images/            # SkiaMediaImageProcessor (SkiaSharp: size, EXIF turn, WebP preview)
    DependencyInjection/
  Media.Api/Media.Api/
    Folders/MediaFoldersController.cs   # /api/media/folders
    Files/MediaFilesController.cs       # /api/media/files
  Media.Contracts/Media.Contracts/{Files,Imports,Storage}/   # IMediaFileReader, IMediaFileUploader, IMediaUrlImporter, ICdnStorage
  Media.Infrastructure/Media.Infrastructure/Contracts/       # their implementations
src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Adapters/Media/ProductMediaReader.cs  # Catalog IProductMediaReader → Media.Contracts
src/Modules/Media/Media.Infrastructure/Media.Infrastructure/Adapters/Catalog/MediaUsageReader.cs  # Media IMediaUsageReader → Catalog.Contracts ICatalogUsage + Crm.Contracts IEmbroideryCalculationReader
src/Bootstrapper/Host.Seed/Jobs/Sync/MediaThumbnailsSyncTask.cs # sync step media-thumbnails
```

## Structure diagram

```mermaid
flowchart LR
  Admin[Brevi Admin] --> Controllers[Media controllers]
  Controllers --> UseCases[Mediator use cases]
  UseCases --> Db[(media schema)]
  UseCases --> Cdn[AdmToolsMediaStorage → CDN]
  UseCases --> Usage[IMediaUsageReader]
  Usage -.adapter.-> CatalogQuery[Catalog.Contracts ICatalogUsage]
  CatalogHandlers[Catalog product handlers] --> Reader[IProductMediaReader]
  Reader -.Media.Contracts.-> MediaQuery[Media GetMediaFilesByIdsQuery]
```

## Tests

- `tests/UnitTests/Media/` — folder tree and handler scenarios on SQLite in memory with a fake CDN; `MediaImageTests` for size, previews, EXIF turn and backfill.
- `tests/ArchitectureTests/MediaDependencyTests.cs` — layer references, own schema and history.
- `tests/IntegrationTests/MediaLibraryMigrationTests.cs` — copy of old Catalog photos (needs `BREVIERP_CATALOG_TEST_CONNECTION`).

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Media`.

**Data:** [media schema](../../architecture/data-model/media.md)
