# CNT-007 — «Контент → Сторінки сайту»: search texts of the fixed site pages (Content module)

**Status:** accepted by the owner 2026-11-11 (stage 3 of the site SEO plan: SEO of the site pages in the Admin); stage 1 (this repository) implemented 2026-11-11. **Date:** 2026-11-11. **Module:** `Content`, schema `content`.

## Why

The titles and descriptions that search engines show for the 16 fixed pages of the new site (home, catalog, wholesale, contacts and others) live in the site code. Every change needs a developer and a release. The SEO audit of 2026-11-11 listed them to move into the Admin; the menu item «Сторінки сайту» already waits for them.

## Decision

A row per fixed page in `content.SitePages`, keyed by a stable page key the site knows (`home`, `wholesale`, …). The list of pages is fixed in code (`SitePageCatalog`): staff edit texts, not pages. The migration seeds the texts the site shows today, so the page opens filled. The site keeps its own texts as the fallback for an empty text or an unavailable API. Product, category and article pages keep their SEO in their own sections. Visible page texts and FAQ came later in [CNT-010](../010-site-page-texts/README.md).

## Rules

- R-01: Per page and language (uk, ru): title (≤ 200) and description (≤ 320). Spaces and line breaks are joined to one line; an empty text gives the page back the site's own text.
- R-02: The migration `AddSitePages` inserts the current site texts with revision 1.
- R-03: Save replaces the four texts of one page with a revision check (0 for a page never saved); an unknown key is 404. The public endpoint lists the saved texts by key; the server reads them at most once a minute, answers may be kept five minutes.
- R-04: Access: page key `content-site-pages` («Сторінки сайту», group «Контент»), actions View and Edit; defaults Director and Admin both, Manager view.
- R-05: The home title is shown as written (it carries the brand); the site adds « — Brevi» to the other titles.

## Scenarios

- SC-01: Staff open «Контент → Сторінки сайту» and see every fixed page with its address and texts.
- SC-02: Staff change a title or description in Ukrainian or Russian and save; a text that is too long is refused.
- SC-03: Within minutes the site shows the new title and description of that page.

## Design

Admin API `/api/content/site-pages` ([contract](../../../contracts/content/content-site-pages.openapi.yaml)): `getSitePages`, `saveSitePage`. Public API `/api/public/site-pages` ([contract](../../../contracts/content/content-public-site-pages.openapi.yaml)): `getPublicSitePages`. Migration `AddSitePages`.

| Path | Responsibility | State |
| --- | --- | --- |
| `Content.Domain/SitePages/SitePage.cs`, `SitePageCatalog.cs` | entity and limits; the fixed pages with names and addresses | new |
| `Content.Application/SitePages/GetList`, `Save`, `Shared` | admin list and save, messages, mapping | new |
| `Content.Application/PublicSitePages` | public read kept a minute | new |
| `Content.Api/SitePages`, `Content.Api/PublicSitePages` | controllers | new |
| `Content.Infrastructure/.../SitePageConfiguration.cs`, migration `AddSitePages` | mapping, seed of the current texts | new |
| `Identity.Domain/Authorization/AccessCatalog.cs` | `content-site-pages` | changed |

## Stages (one PR each)

1. Table, admin and public API (this repository).
2. Admin page «Сторінки сайту» and the site reading the texts (brevi-web).

## Tests

`tests/UnitTests/Content/SitePagesTests.cs`, `SitePagesContractTests.cs`, `tests/HostApiTests/ApiAccessTests.cs`.
