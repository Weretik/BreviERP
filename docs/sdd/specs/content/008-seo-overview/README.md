# CNT-008 — «Контент → SEO-огляд»: what the site misses for search (Content module)

**Status:** accepted by the owner 2026-11-11 (stage 4 of the site SEO plan); stage 1 (this repository) implemented 2026-11-11. **Date:** 2026-11-11. **Module:** `Content`, schema `content`.

## Why

SEO texts now live in four places of «Контент» (product pages, categories, articles, site pages) plus the site settings. Staff need one page that says what is still missing, instead of opening every card.

## Decision

A read-only query over the data «Контент» already owns: no new table. Only problems are listed (no "all OK" marks). Each problem links back to the section where it is fixed.

## Rules

- R-01: Checked pages: products and categories shown on the site (a category also needs its parent shown; a product also needs an active catalogue model), articles shown on the site, and the fixed site pages except «Заявка» (not in search).
- R-02: Per page and language: no SEO title (`NoTitle`) or longer than 60 characters (`TitleTooLong`); no SEO description (`NoDescription`) or longer than 160 (`DescriptionTooLong`); the shown title equals another checked page's in the same language, ignoring case (`DuplicateTitle`); no Russian texts at all (`NoRussian`, for the page).
- R-03: Site-wide: no Google Analytics or Tag Manager id (`NoAnalytics`), no Google or Bing verification code, no sharing picture.
- R-04: Access: page key `content-seo-overview` («SEO-огляд», group «Контент»), action View only; defaults Director, Admin and Manager.

## Scenarios

- SC-01: Staff open «Контент → SEO-огляд» and see the site-wide problems and, per area, how many pages are on the site and how many have problems.
- SC-02: Staff filter the list by area or problem and open the page's section to fix it.
- SC-03: After a fix the page leaves the list.

## Design

Admin API `GET /api/content/seo-overview` ([contract](../../../contracts/content/content-seo-overview.openapi.yaml)), operation `getSeoOverview`.

| Path | Responsibility | State |
| --- | --- | --- |
| `Content.Application/Contracts/SeoOverviewResponses.cs` | response and problem enums | new |
| `Content.Application/SeoOverview/SeoChecks.cs` | the per-page checks and repeated titles | new |
| `Content.Application/SeoOverview/GetSeoOverviewQuery.cs` | collects pages of the four areas and the site problems | new |
| `Content.Api/SeoOverview/SeoOverviewController.cs` | controller, policy `access:content-seo-overview` | new |
| `Identity.Domain/Authorization/AccessCatalog.cs` | `content-seo-overview` | changed |

## Stages (one PR each)

1. Query and admin API (this repository).
2. Admin page «SEO-огляд» (brevi-web).

## Tests

`tests/UnitTests/Content/SeoOverviewTests.cs`, `SeoOverviewContractTests.cs`, `tests/HostApiTests/ApiAccessTests.cs`.
