# Медіафайли каталогу

**Статична звірка з кодом:** 2026-10-16. **Модуль:** catalog.

Завантаження, список і видалення файлів для товарів.

## Будова й зв'язки

[CatalogMediaController](../../../../src/Modules/Catalog/Catalog.Api/Catalog.Api/Controllers/CatalogMediaController.cs) викликає [upload handler](../../../../src/Modules/Catalog/Catalog.Application/Catalog.Application/Features/Media/Upload/UploadMediaFileCommandHandler.cs): створює pending [MediaFile](../../../../src/Modules/Catalog/Catalog.Domain/Catalog.Domain/Media/Entities/MediaFile.cs), завантажує в storage через адаптер і позначає uploaded. Метадані зберігає [CatalogDbContext](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/DataBase/CatalogDbContext.cs); файл і запис БД мають різні життєві цикли.

## Структура в коді

Наведено перевірені точки входу та ключові складові фічі; дерево не перелічує всі допоміжні файли.

```text
src/
└── Modules/Catalog/
    ├── Catalog.Api/Catalog.Api/Controllers/CatalogMediaController.cs  # HTTP-вхід
    ├── Catalog.Application/Catalog.Application/Features/Media/
    │   ├── Delete/DeleteMediaFileCommandHandler.cs  # обробник сценарію
    │   ├── GetList/GetMediaFilesQueryHandler.cs  # обробник сценарію
    │   └── Upload/UploadMediaFileCommandHandler.cs  # обробник сценарію
    ├── Catalog.Domain/Catalog.Domain/Media/Entities/MediaFile.cs  # доменна модель
    └── Catalog.Infrastructure/Catalog.Infrastructure/DataBase/CatalogDbContext.cs  # контекст даних
```

## Схема будови

```mermaid
flowchart LR
    A["CatalogMediaController"] --> B["UploadMediaFileCommandHandler"]
    B --> C["MediaFile: pending"]
    B --> D["IMediaStorageService"]
    D --> E["Файл у сховищі"]
    B --> F["MediaFile: uploaded"]
    B --> G["ICatalogRepository<MediaFile>"]
    G -.-> H["CatalogEfRepository"]
    H --> I["CatalogDbContext"]
```

## Пов'язані джерела

[OpenAPI](../../contracts/catalog/product-dependencies.openapi.yaml) · [SDD](../../specs/catalog/media-files/README.md).
