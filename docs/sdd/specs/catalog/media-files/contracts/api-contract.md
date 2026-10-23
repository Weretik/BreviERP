# API change: Catalog media files

**Module:** catalog\
**Status:** verified in code\
**Contract status:** implemented

## Goal

Provide an API to upload, list, and delete images that will later be used by Product.

## Routes and methods

```http
GET    /api/catalog/media
POST   /api/catalog/media       Content-Type: multipart/form-data
DELETE /api/catalog/media/{id}
```

## Request and response

| Operation | Request | Successful response | Errors |
| --- | --- | --- | --- |
| List | no parameters | `200` list of IDs, original file name, public URL, content type, storage key, and status | — |
| Upload | form-data field `file` | `200` with `mediaFileId`, `storageKey`, `publicUrl`, `contentType`, `originalFileName` | `400` validation/signature mismatch |
| Delete | route `id:int` | `204` | `400`, `404` |

File requirements: JPEG/PNG/WebP, positive size, no larger than `CatalogMediaUpload:MaxFileSizeBytes`; the signature must match the declared content type.

## Security and side effects

- Current access: `AllowAnonymous`.
- Upload is not idempotent: each successful retry creates a new storage key.
- Delete has side effects in external storage and the database.
