# Catalog media files

**Module:** catalog\
**Status:** verified\
**Owner:** Catalog\
**Created:** 2026-07-27\
**Related:** `CatalogMediaController`, `MediaFile`, `IMediaStorageService`

## Goal

As a catalog API user, I want to upload, view, and delete images so that ready media files can be safely linked to products.

## Scope

- In scope: JPEG/PNG/WebP, media-file metadata and status, upload/list/delete API, external `adm.tools` storage.
- Out of scope: endpoint authorization (current `AllowAnonymous` was agreed separately), image processing/resizing, linking photos to Product.
- Assumption: the storage API is available; credentials are supplied only through configuration.
- Dependencies: PostgreSQL, `AdmToolsMediaStorageService`, `CatalogMediaUpload`, and `AdmToolsStorage` options.

## Scenarios

1. Given a valid JPEG/PNG/WebP, when the client sends a multipart upload, the file is stored, `MediaFile` becomes `Ready`, and the client receives an ID and public URL.
2. Given a mismatch between MIME type and signature, when the client uploads a file, the handler is not called and a validation result is returned.
3. Given an existing file, when the client deletes it by ID, the object is removed from storage and the database.

## Contract and compatibility

- API contract: [contracts/api-contract.md](contracts/api-contract.md).
- New routes: `GET|POST /api/catalog/media`, `DELETE /api/catalog/media/{id}`.
- Authorization: endpoints currently have explicit `AllowAnonymous`; this is an intentional temporary contract.
- Idempotency: upload does not support `Idempotency-Key`; a repeated upload creates a separate file. Delete returns `NotFound` for a missing ID.

## Implementation phases

| Phase | Status | Required? | Result |
| --- | --- | --- | --- |
| [01 Domain](phases/01-domain.md) | verified | yes | `MediaFile`, ID, statuses, and invariants |
| [02 Infrastructure](phases/02-infrastructure.md) | verified | yes | EF mapping and `adm.tools` adapter |
| [03 Application](phases/03-application.md) | verified | yes | upload/list/delete CQRS and validation |
| [04 API](phases/04-api.md) | verified | yes | multipart endpoints and Result mapping |
| [05 Swagger manual test](phases/05-swagger-manual-test.md) | planned | yes | reproducible manual verification |
| [06 Frontend handoff](phases/06-frontend-handoff.md) | planned | yes | stable contract for the product page |

## Files

- Created/changed: `Catalog.Domain/Media`, `Catalog.Application/Features/Media`, `Catalog.Infrastructure/Services/AdmToolsMediaStorageService`, `Catalog.Api/Controllers/CatalogMediaController`.
- Do not change without a separate specification: secrets, deployment, and global serialization configuration.

## Acceptance criteria

- [x] Only JPEG, PNG, and WebP with matching file signatures are accepted.
- [x] A file must complete the `PendingUpload → Ready` lifecycle before it can be used in Product.
- [x] Storage key is unique; the database has a unique index for it.
- [x] Upload/list/delete use Mediator.
- [ ] API responses and manual Swagger cases are verified against configured storage.

## Verification

- Build: `dotnet build .\BreviERP.sln --no-restore` — succeeded 2026-07-27.
- Tests: `dotnet test .\BreviERP.sln --no-build` — succeeded, UnitTests 1/1, IntegrationTests 1/1.
- Manual verification: not performed; requires available `adm.tools` and test credentials.

## Risks and open questions

- An orphaned object may remain if a technical failure occurs after a successful upload to storage but before the database is updated.
- Delete removes the file from storage first, then its database record; a recovery strategy is needed for partial failure.
- Before production deployment, define the access policy and retention period for unattached media files.

## Change log

- 2026-07-27 — Recorded the implemented media lifecycle and moved signature validation to Application.
