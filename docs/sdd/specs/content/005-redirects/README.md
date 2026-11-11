# CNT-005 — «Контент → Переадресації»: 301 redirects of the public site (Content module)

**Status:** accepted by the owner 2026-11-11 (SEO work in stages, redirects first); stage 1 (this repository: table, admin API, old-site transfer, automatic redirects on address change, public API) implemented 2026-11-11. **Date:** 2026-11-11. **Module:** `Content`, schema `content`. Continues [CNT-002](../002-product-pages/README.md), [CNT-003](../003-site-categories/README.md) and [CNT-004](../004-articles/README.md).

## Why

The new site replaces `brevi.com.ua`, which has 399 indexed addresses (products, categories, articles, city and information pages, and their `/ru/` copies). The new site redirects only `/` to `/uk`; every other old address answers 404, so search engines would drop the pages and the links people saved would break. When staff change the address of a product, a category or an article, the old address breaks the same way. The SEO audit of 2026-11-11 named this the first thing to fix, and the owner asked for SEO settings to be managed in the Admin.

## Decision

Redirects are owned by Content: they lead to pages Content owns, and a change of a page address must add one in the same save. The table keeps normalized paths (lower case, no host, query, fragment or closing slash), so `https://brevi.com.ua/Kostyum/` and `/kostyum` are the same address. The public site reads the whole list from an anonymous endpoint, cached, and answers 301 for an address it does not know; a redirect never hides a live page. A chain (a page renamed twice) is followed on the server, so a visitor gets one 301. The alternative (redirects in `next.config` or a Cloudflare rule) would need a developer for every change.

## Rules

- R-01: An address is normalized as above; percent-encoding is decoded, repeated slashes go, at most 500 characters, no spaces. The site root is never an old address.
- R-02: A redirect has `FromPath` (unique), `ToPath` (a page of the new site: `/uk`, `/ru` or below them), `Source` (`Manual`, `OldSite`, `SlugChange`), `Note` (≤ 200), `UpdatedAtUtc`, `UpdatedBy`. From and to differ. At most 10 000 redirects.
- R-03: Staff add, change and delete redirects; a taken old address or a pair that would make a two-step loop is a 409. An edit by staff makes the redirect `Manual`. Access: page key `content-redirects` («Переадресації», group «Контент»), actions View, Create, Edit, Delete; defaults Director and Admin all, Manager view.
- R-04: The old-site transfer takes a file of at most 3 000 addresses, each with an optional target. Without a target the server finds the page: an address under `/ru/` is resolved without that prefix and goes to the Russian page; the last part of the address is tried as an article, a site category and the old-site link of a Catalog product (its page, else its first site category); then the deepest part that is a site category. What is not found is reported, not saved. An existing redirect is kept unless «перезаписати» is asked; a `Manual` one is always kept. The preview writes nothing; a repeated run changes nothing that is right.
- R-05: When staff change the address of a product page, a site category or an article, the old address of both languages gets a `SlugChange` redirect to the new one, redirects that led to the old address lead to the new one, and a redirect from the new address is removed.
- R-06: The public list gives every redirect with the final page of its chain; a loop or a chain over five steps is left out. The server reads the table at most once a minute; answers may be kept five minutes.

## Scenarios

- SC-01: Staff open «Контент → Переадресації» and see every redirect with its source, search it and filter by source.
- SC-02: Staff add, change or delete a redirect in the side panel; a wrong address, a target outside the new site or a taken address is refused with a Ukrainian message.
- SC-03: Staff transfer the old-site addresses from the prepared file: preview with what each address resolves to and what was not found, then apply.
- SC-04: The site answers 301 to the new page for every known old address, including `/ru/` ones, and 404 for the rest.
- SC-05: Staff change the address of a product, a category or an article; the old address keeps working.

## Design

Table `content.SiteRedirects` (migration `AddSiteRedirects`): Id identity, FromPath unique, ToPath indexed, Source int, Note, UpdatedAtUtc, UpdatedBy. Admin API `/api/content/redirects` ([contract](../../../contracts/content/content-redirects.openapi.yaml)): `getSiteRedirects`, `createSiteRedirect`, `saveSiteRedirect`, `deleteSiteRedirect`, `importOldSiteRedirects`. Public API `/api/public/redirects` ([contract](../../../contracts/content/content-public-redirects.openapi.yaml)): `getPublicRedirects`, anonymous, memory cache of 60 s.

| Path | Responsibility | State |
| --- | --- | --- |
| `Content.Domain/Redirects/SiteRedirectRules.cs` | normalizing, limits, site path checks | new |
| `Content.Domain/Redirects/SiteRedirect.cs` | entity and source | new |
| `Content.Application/Redirects/GetList`, `Save`, `Delete` | admin list and edits with validators | new |
| `Content.Application/Redirects/ImportOldSite` | file shape, resolver, transfer handler | new |
| `Content.Application/Redirects/Shared/SiteRedirectWriter.cs` | redirects on an address change (R-05) | new |
| `Content.Application/PublicRedirects` | public list, chain flattening, cache | new |
| `ProductPages/Save`, `SiteCategories/Save`, `Articles/Save` | call the writer when the slug changed | changed |
| `Content.Infrastructure/Persistence/Configurations/SiteRedirectConfiguration.cs` | mapping | new |
| `Content.Api/Redirects`, `Content.Api/PublicRedirects` | controllers | new |
| `Identity.Domain/Authorization/AccessCatalog.cs` | `content-redirects` | changed |

## Stages (one PR each)

1. Table, admin API, transfer, automatic redirects, public API (this repository).
2. Admin page «Контент → Переадресації», the site answering 301 from the list, the transfer file of the old addresses (brevi-web).

## Tests

`tests/UnitTests/Content/SiteRedirectsTests.cs` (normalizing, edits, loop, address changes of products, categories and articles, transfer with every resolver rule, dry run, overwrite and `Manual`, public chains and loops, cache), `SiteRedirectsContractTests.cs` (contract shapes, policy, «Права доступу» defaults), `tests/HostApiTests/ApiAccessTests.cs` (the public endpoint is the only new anonymous one).
