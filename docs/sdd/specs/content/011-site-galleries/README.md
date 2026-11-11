# CNT-011 — «Контент → Наші роботи» and «Контент → Сертифікати»: site pictures in the «Фото» library (Content module)

**Status:** accepted by the owner 2026-11-11 (stage 7 of the site SEO plan); stage 1 (this repository) implemented 2026-11-11. **Date:** 2026-11-11. **Module:** `Content`, schema `content`.

## Why

The site pages «Наші роботи» (82 photos of sewn clothing) and «Сертифікати» (8 documents with titles, notes and standards) keep their pictures among the site's own files and their lists in the site code. A new photo or certificate needs a developer and a release, and the pictures are not in the CDN library the rest of the Admin uses.

## Decision

One row per page in `content.SiteGalleries` with the list as JSON, saved as a whole with a revision check. The migration seeds the lists the site shows today; their pictures stay files of the site (`/assets/…`) until staff press «Перенести у бібліотеку фото», which downloads them into the «Фото» library in steps of 10 and keeps the order and texts. New pictures come only from the library. The site keeps its own lists as the fallback.

## Rules

- R-01: «Наші роботи»: up to 300 pictures, each with an optional description in two languages (≤ 300). «Сертифікати»: up to 40 documents, each with a Ukrainian title (Russian optional, ≤ 300), an optional note (≤ 600) and up to 10 standards (≤ 200 each).
- R-02: The migration `AddSiteGalleries` inserts the current site lists with revision 1.
- R-03: Save replaces the list with a revision check. A site file may stay but not be added; a newly added library photo must exist; one picture only once. Wrong items are refused with a Ukrainian reason.
- R-04: Access: page keys `content-works` («Наші роботи») and `content-certificates` («Сертифікати»), group «Контент», actions View and Edit; defaults Director, Admin and SEO specialist both, Manager view.
- R-05: The transfer downloads `https://brevi-site.pnevman.workers.dev/assets/…` into the library folder «Сайт / Наші роботи» or «Сайт / Сертифікати», marks each photo with source `site-assets:<path>` so a repeat does not download it again, and reports pictures that failed.
- R-06: The public endpoint gives both lists: a site file by its site path, a library photo by its CDN address; a deleted photo is left out. The server reads them at most once a minute; answers may be kept five minutes.

## Scenarios

- SC-01: Staff open «Наші роботи» or «Сертифікати» and see the pictures the site shows.
- SC-02: Staff add a photo from the library, reorder, remove or caption pictures and save; a wrong item is refused with the reason.
- SC-03: Staff press «Перенести у бібліотеку фото»; the pictures move into the library step by step and failed ones are named.
- SC-04: Within minutes the site shows the new list.

## Design

Admin API `/api/content/works` and `/api/content/certificates` ([contract](../../../contracts/content/content-galleries.openapi.yaml)): get, save (PUT) and `POST …/transfer`. Public API `/api/public/galleries` ([contract](../../../contracts/content/content-public-galleries.openapi.yaml)). Migration `AddSiteGalleries`.

| Path | Responsibility | State |
| --- | --- | --- |
| `Content.Domain/SiteGalleries/SiteGallery.cs`, `SiteGalleryItem.cs` | entity, item records, limits, site-file path | new |
| `Content.Application/SiteGalleries/Get`, `Save`, `Transfer`, `Shared` | admin read, save, transfer, messages, addresses | new |
| `Content.Application/PublicSiteGalleries` | public read kept a minute | new |
| `Content.Api/SiteGalleries`, `Content.Api/PublicSiteGalleries` | controllers | new |
| `Content.Infrastructure/.../SiteGalleryConfiguration.cs`, migration `AddSiteGalleries` | mapping with a jsonb list, seed | new |
| `Identity.Domain/Authorization/AccessCatalog.cs` | `content-works`, `content-certificates` | changed |

## Stages (one PR each)

1. Table, admin and public API, transfer (this repository).
2. Admin pages «Наші роботи» and «Сертифікати» and the site reading the lists (brevi-web).

## Tests

`tests/UnitTests/Content/SiteGalleriesTests.cs`, `SiteGalleriesContractTests.cs`, `tests/UnitTests/Identity/AccessRightsTests.cs`, `tests/HostApiTests/ApiAccessTests.cs`.
