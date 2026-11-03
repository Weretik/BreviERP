# Media schema

Code model reviewed 2026-11-03. `Media.Infrastructure.Persistence.MediaDbContext` owns schema `media`; migration history is `media.__EFMigrationsHistory`. Migration `InitialMedia` creates the tables and copies uploaded photos from `catalog."MediaFiles"` ([MED-001](../../specs/media/001-media-library/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| Folders | Generated int id. Name (120), nullable ParentId FK → Folders (restrict), CreatedAt, UpdatedAt. Index ParentId. Sibling names are unique ignoring case (checked by the handlers) |
| Files | Generated int id, the same ids as the old `catalog.MediaFiles`. FileName (255), Alt (300), nullable FolderId FK → Folders (restrict), ContentType, SizeInBytes, StorageProvider, unique StorageKey, PublicUrl, Width, Height, CreatedAt, UpdatedAt. Index FolderId |

`catalog.ProductPhotos.MediaFileId` stores a `media.Files.Id` as a value; there is no foreign key between the schemas.
