# CNT-006 — «Контент → Налаштування SEO сайту»: site-wide SEO settings (Content module)

**Status:** accepted by the owner 2026-11-11 (SEO settings managed in the Admin; connecting the services is the SEO specialist's job, the system only gives the fields); stage 1 (this repository) implemented 2026-11-11. **Date:** 2026-11-11. **Module:** `Content`, schema `content`.

## Why

The SEO audit of 2026-11-11 found no analytics and no search-console verification on the new site, and no picture for sharing a page that has none of its own. Each of them would need a developer to change the code. The owner asked for these settings in the Admin, so the SEO specialist can connect the services without a release.

## Decision

One row of settings in Content (table `content.SiteSeoSettings`, check `Id = 1`), next to the other site content. The public site reads them from an anonymous endpoint and places the codes on every page; nothing secret is stored (these values are visible in every page's source anyway). The sharing picture is a photo of the «Фото» library by id, read through `IContentMediaReader`, so it lives on our CDN. Indexing of the whole site stays a server setting of the site (`SITE_INDEXING`), not an Admin switch, so a wrong click cannot hide the site from search.

## Rules

- R-01: Fields: `GoogleAnalyticsId` (`G-` + 4–15 letters or digits), `GoogleTagManagerId` (`GTM-` + 4–12), `GoogleSiteVerification` and `BingSiteVerification` (only the `content` value of the meta tag: letters, digits, `-`, `_`, 8–100), `DefaultOgPhotoId` (a photo of the library). Ids are kept in upper case; an empty field removes the code.
- R-02: Save replaces the settings as a whole with a revision check (0 before the first save); a photo missing from the library is refused.
- R-03: Access: page key `content-seo-settings` («Налаштування SEO сайту», group «Контент»), actions View and Edit; defaults Director and Admin both, Manager view.
- R-04: The public endpoint gives the codes and the picture URL with its size; the server reads them at most once a minute, answers may be kept five minutes.

## Scenarios

- SC-01: Staff open «Контент → Налаштування SEO сайту» and see the codes and the picture.
- SC-02: Staff paste the codes, choose a picture from «Фото» and save; a code of the wrong shape is refused with a Ukrainian message that says what is expected.
- SC-03: The site places the verification meta tags, the analytics scripts and the default sharing picture on every page.

## Design

Admin API `/api/content/seo-settings` ([contract](../../../contracts/content/content-seo-settings.openapi.yaml)): `getSiteSeoSettings`, `saveSiteSeoSettings`. Public API `/api/public/site-settings` ([contract](../../../contracts/content/content-public-site-settings.openapi.yaml)): `getPublicSiteSettings`. Migration `AddSiteSeoSettings`.

| Path | Responsibility | State |
| --- | --- | --- |
| `Content.Domain/SiteSettings/SiteSeoSettings.cs` | entity, formats of the codes | new |
| `Content.Application/SiteSettings/Get`, `Save`, `Shared` | admin read and save, picture read, messages | new |
| `Content.Application/PublicSiteSettings` | public read kept a minute | new |
| `Content.Api/SiteSettings`, `Content.Api/PublicSiteSettings` | controllers | new |
| `Content.Infrastructure/.../SiteSeoSettingsConfiguration.cs` | mapping with the single-row check | new |
| `Identity.Domain/Authorization/AccessCatalog.cs` | `content-seo-settings` | changed |

## Stages (one PR each)

1. Table, admin and public API (this repository).
2. Admin page and the site placing the codes (brevi-web), together with quick SEO fixes of the site.

## Tests

`tests/UnitTests/Content/SiteSeoSettingsTests.cs`, `SiteSeoSettingsContractTests.cs`, `tests/HostApiTests/ApiAccessTests.cs`.
