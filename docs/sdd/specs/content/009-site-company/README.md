# CNT-009 — «Контент → Контакти та компанія»: company facts of the public site (Content module)

**Status:** accepted by the owner 2026-11-11 (stage 5 of the site SEO plan); stage 1 (this repository) implemented 2026-11-11. **Date:** 2026-11-11. **Module:** `Content`, schema `content`.

## Why

The company phone, email, address, working hours, client managers, payment details and profile links live in the site code (`COMPANY` in brevi-web). They show in the header, footer, «Контакти», «Доставка та оплата», the agreement and the structured data search engines read (`Organization`, `LocalBusiness`). A new manager or a changed phone needs a developer and a release.

## Decision

One row in `content.SiteCompany`, edited as a whole on one Admin page. The migration seeds what the site shows today. Small nested parts (two-language texts, the schedule, managers, bank details, links) are JSON text columns: they are always read and written together and nothing queries inside them. The site keeps its own facts as the fallback when the API has none or is unavailable.

## Rules

- R-01: Fields: the company phone (one, Ukrainian number), email, city and street in Ukrainian and Russian, the map query, the schedule (working days and one time range), up to 10 managers (name in two languages, a photo from «Фото» or the site, 1–3 phones, an optional email), payment details (recipient and bank in two languages, ЄДРПОУ, IBAN) and Instagram, YouTube and Facebook links (https, optional).
- R-02: The migration `AddSiteCompany` inserts the current site facts with revision 1.
- R-03: Save replaces everything with a revision check. Phones are stored as the site shows them (`050 469 86 75`), the IBAN grouped as banks print it, spaces in texts joined. Wrong fields are refused with a Ukrainian reason.
- R-04: Access: page key `content-company` («Контакти та компанія», group «Контент»), actions View and Edit; defaults Director and Admin both, Manager view.
- R-05: The public endpoint gives the same facts with each phone also in the `tel:` form (`+380504698675`); 404 before the facts exist. The server reads them at most once a minute; answers may be kept five minutes.

## Scenarios

- SC-01: Staff open «Контент → Контакти та компанія» and see the facts the site shows.
- SC-02: Staff change a phone, the hours or a manager and save; a wrong number, IBAN or link is refused with the reason.
- SC-03: Within minutes the site shows the new facts in the header, footer, «Контакти» and the structured data.

## Design

Admin API `/api/content/company` ([contract](../../../contracts/content/content-company.openapi.yaml)): `getSiteCompany`, `saveSiteCompany`. Public API `/api/public/company` ([contract](../../../contracts/content/content-public-company.openapi.yaml)): `getPublicSiteCompany`. Migration `AddSiteCompany`.

| Path | Responsibility | State |
| --- | --- | --- |
| `Content.Domain/SiteCompany/SiteCompanyInfo.cs` | entity, nested records, phone and IBAN formats, checks | new |
| `Content.Application/SiteCompany/Get`, `Save`, `Shared` | admin read and save, messages, mapping | new |
| `Content.Application/PublicSiteCompany` | public read kept a minute | new |
| `Content.Api/SiteCompany`, `Content.Api/PublicSiteCompany` | controllers | new |
| `Content.Infrastructure/.../SiteCompanyInfoConfiguration.cs`, migration `AddSiteCompany` | mapping with JSON columns, seed | new |
| `Identity.Domain/Authorization/AccessCatalog.cs` | `content-company` | changed |

## Stages (one PR each)

1. Table, admin and public API (this repository).
2. Admin page «Контакти та компанія» and the site reading the facts (brevi-web).

## Tests

`tests/UnitTests/Content/SiteCompanyTests.cs`, `SiteCompanyContractTests.cs`, `tests/HostApiTests/ApiAccessTests.cs`.
