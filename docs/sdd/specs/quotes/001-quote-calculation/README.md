# QTE-001 — «КП»: commercial proposals (Quotes module)

**Status:** specification 2026-11-04; stage 1 next. **Date:** 2026-11-03 (idea), 2026-11-04 (specification). **Module:** new module `Quotes`, schema `quotes`, own migration history `quotes.__EFMigrationsHistory` ([module rules](../../../standards/module-rules.md)).

## Why

Managers build commercial proposals (КП) in the client's old Calculator cabinet. Brevi now holds everything a КП is built from: products with fabric variants and three published prices, PPE with live prices, short «для КП» descriptions of products and fabrics, current site links, photos and clients from KeepinCRM. The product owner asked on 2026-11-04 for the first working ways to create a КП in the Admin, with the screens and rules of the old cabinet (owner's screenshots and answers in the project thread «Сделать создание КП в Brevi»).

## Decision: a new module, not the Crm quotes

Crm already contains versioned quotes, introductions, issuer settings, PDF and Gmail delivery built during the earlier port ([CRM-001](../../crm/001-clients-quotes-custom-sewing/README.md)). That code was written against the old catalogue, was never used by staff, and is about 700 files with journals, idempotency stores and archive tooling that a КП does not need.

| Option | For | Against |
| --- | --- | --- |
| **A. New module `Quotes` (chosen, pending the owner's confirmation)** | Small, matches the old cabinet screen by screen, owns only КП data, reads Catalog/Reference/Crm through adapters | Rewrites what Crm already has; Crm quotes are removed later |
| B. Finish the Crm quotes | Code exists, already in the Admin menu | Keeps the complexity; screens and rules differ from the cabinet; every change touches a large unverified codebase |

Crm keeps clients (KeepinCRM). Proven pieces of Crm (PDF rendering, Gmail sending) may be moved into `Quotes` in their stages after review, not referenced.

## Kinds of КП

| Kind | What the manager does | Prices shown |
| --- | --- | --- |
| **Під запит** (request) | Picks a client, adds products: sewing model with one or more fabric variants, catalogue PPE, one-off PPE; later a model from scratch | Up to three per variant: до 9 шт., 10–40 шт., від 41 шт.; each can be hidden |
| **Для знайомства** (introduction) | Picks a layout (general = summer + winter page, summer, winter), three more models per page and one fabric per model, a recipient and a letter text | Only від 41 шт. |

## Rules

### Stage 1 — creating a КП «під запит»

- **R-01:** Any Manager, Director or Admin creates a КП. Its author and responsible person are the creator. A Manager sees and changes only КП where they are responsible; Director and Admin see and change all.
- **R-02:** Director and Admin can make any active staff member (Manager, Director, Admin) responsible for a КП, including themselves. The author never changes.
- **R-03:** The number is assigned on the first save as `<n>-<month>-<year>` (Kyiv date), where `n` counts КП of that month from 1. It never changes.
- **R-04:** A КП has an internal name of the client or request (required, up to 200 characters, only staff see it), an optional KeepinCRM client from Crm, an optional recipient name and company name (up to 200), a comment for the client and an internal manager note (up to 4000 each).
- **R-05:** Items keep their order. Item kinds: sewing model, catalogue PPE, one-off PPE. A sewing model or catalogue PPE must be active in Catalog when added.
- **R-06:** An item copies its name, «для КП» description, site link, photo and season from Catalog when added. The description is editable inside the КП only (up to 4000); Catalog does not change.
- **R-07:** A sewing item has one or more fabric variants, each an active variant (Article) of that model, without repeats. A variant copies the fabric name, composition, density, the fabric «для КП» description and the three published prices. A variant without a published price cannot be added; the message names the reason. One variant may be marked «Рекомендований варіант».
- **R-08:** A catalogue PPE item has one price row with the three current prices. The manager may replace any of them with a manual positive price and a reason (up to 500); the КП keeps the source price, the manual price, the reason, who and when.
- **R-09:** A one-off PPE item has a name (required), description, optional supplier link and three manual prices; at least one price must be positive. It does not create a product.
- **R-10:** Each of the three prices of a variant or PPE row has a «show in КП» flag. A saved КП must show at least one positive price per item.
- **R-11:** Prices copied into a КП never change by themselves. «Перерахувати за актуальними цінами» reloads the current Catalog prices of all catalogue items, keeps manual PPE prices, and lists what changed (old → new) and what is no longer available.
- **R-12:** A КП is a draft until it is sent (stage 3). Every save checks the version the editor loaded; a stale save is rejected with 409 and the editor offers to reload.

### Stage 2 — settings and PDF

- **R-13:** «Налаштування КП» is visible only to Director and Admin: company requisites (name, ЄДРПОУ, ІПН, IBAN, address, phone, email, site, logo), the contact each staff member shows in a КП (name, phone, email), the email subject and text with marks `{quoteNumber}`, `{managerName}`, `{managerPhone}`, `{managerEmail}`, `{site}`, PDF texts (title, VAT note, call-to-action), the introduction catalogue texts with five «Переваги», and the introduction letter variants.
- **R-14:** «Сформувати КП» saves the КП and produces a PDF from saved data only: shown items, variants and prices, the client texts, the responsible person's contact and the requisites. Internal names, notes, costs, suppliers, manual price reasons and hidden prices never appear in it.

### Stage 3 — sending by Gmail

- **R-15:** Each manager sends from their own Gmail address, connected once by the Director. Sending shows the recipient, subject and text (editable), attaches the PDF of the saved КП and on success marks the КП sent with the date. A failure leaves it a draft and shows the reason.

### Stage 4 — introduction КП

- **R-16:** General layout: page «Літній спецодяг» with the fixed main model «Імпульс» and page «Зимовий спецодяг» with «Прайм»; summer or winter layouts have one page. Each page needs three more distinct active models of that season and one fabric per model with a від 41 шт. price.
- **R-17:** A manager saves a set of models, fabrics and letter variant as a named template; using a template takes current prices.

### Later stages

Editing the composition of a sewing model inside a КП with recalculation and the seamstress wage («Розцінка швачки»), a model from scratch, change history, archive and copy, export for production, import of old Calculator КП (only if the partial export allows it; otherwise start clean, owner 2026-11-04), removal of the Crm quotes.

## Scenarios (stage 1)

| ID | Given | When | Then |
| --- | --- | --- | --- |
| SC-01 | Manager, active sewing model with two priced fabric variants, active PPE | Creates a КП, adds the model with both fabrics and the PPE, saves | Number `n-11-2026`, items in order, descriptions and prices from Catalog, all prices shown |
| SC-02 | Saved КП | Manager edits the description, hides «до 9 шт.», marks a recommended fabric, sets a manual PPE price with a reason, saves and reopens | Changes kept; source PPE price, reason and author kept; Catalog unchanged |
| SC-03 | Variant without published price; inactive model | Manager tries to add them | 409 with the reason; nothing saved |
| SC-04 | Two managers | Manager B lists or opens manager A's КП | Not listed; open returns 404 |
| SC-05 | Director | Lists КП, makes manager B responsible for A's КП | Sees all; B now sees it, A no longer does; author stays A |
| SC-06 | КП saved, then a fabric price rises and the new price is published | Opens the КП; then presses «Перерахувати» | Old prices on open; recalculation shows old → new and saves the new ones; manual PPE price kept |
| SC-07 | Two tabs on one КП | Both save | Second save 409 |
| SC-08 | One-off PPE without any positive shown price | Saves | 400 naming the field |

## Data (stage 1)

Schema `quotes`. Other modules' ids are stored as values, without foreign keys.

| Table | Key columns |
| --- | --- |
| `Quotes` | `Id` (bigint identity), `Number` (unique), `NumberMonth`, `NumberSeq` (unique pair), `Kind`, `Status`, `InternalName`, `CustomerId` (Crm, nullable), `RecipientName`, `CompanyName`, `ClientComment`, `ManagerNote`, `AuthorUserId`, `AuthorName`, `ResponsibleUserId`, `ResponsibleName`, `CreatedAt`, `UpdatedAt`, `Version` (concurrency) |
| `QuoteItems` | `Id`, `QuoteId` (cascade), `Position`, `Kind` (`Sewing`, `Ppe`, `OneOffPpe`), `ProductId` (Catalog, nullable), `Code`, `Name`, `Subtitle`, `Description`, `SiteUrl`, `PhotoUrl`, `Season`, `SupplierUrl` |
| `QuoteItemOptions` | `Id`, `ItemId` (cascade), `Position`, `ArticleId` (Catalog, nullable), `FabricId` (Reference, nullable), `Title`, `Details`, `IsRecommended`, for each tier (`Retail`, `Wholesale`, `Large`): `<Tier>Price`, `<Tier>SourcePrice`, `<Tier>Shown`, plus `ManualReason`, `ManualBy`, `ManualAt`, `PricesTakenAt` |

Money is `numeric(12,2)`, rounded as Catalog publishes it. Migration `InitialQuotes` creates the schema on an empty database; rollback is the previous image (the new schema stays unused). Settings, PDF files, deliveries and introduction templates get their own tables in their stages.

## Reads from other modules (adapters in Host.Api)

| Port (Quotes.Application) | Source | Used for |
| --- | --- | --- |
| `IQuoteCatalogReader` | Catalog products, Articles, published Sewing prices, PPE live prices; Reference fabrics (`ProposalDescription`, composition, density); Media photo URL | Picker search and adding/recalculating items |
| `IQuoteCustomerReader` | Crm customers | Client picker and name |
| `IQuoteStaffReader` | Identity users and roles | Responsible person list, names |

The sewing price rule follows `Host.Api/Crm/QuoteCatalogPriceReader.cs`: only a current publication whose calculation matches the composition is a price.

## API (stage 1)

Contract: `docs/sdd/contracts/quotes/quotes.openapi.yaml` (written in TS-01). Base `/api/v1/quotes`; every operation needs a signed-in Manager, Director or Admin; ownership is checked in the handlers.

| Operation | Endpoint |
| --- | --- |
| `getQuotes` | `GET /api/v1/quotes?search=&responsibleUserId=&page=&pageSize=` (search by number, internal name, company; 50 per page, newest first) |
| `getQuote` | `GET /api/v1/quotes/{id}` |
| `createQuote` | `POST /api/v1/quotes` → 201 with the number |
| `saveQuote` | `PUT /api/v1/quotes/{id}` with `version`; the whole editable content |
| `deleteQuoteDraft` | `DELETE /api/v1/quotes/{id}?version=` (draft only) |
| `setQuoteResponsible` | `PUT /api/v1/quotes/{id}/responsible` (Director, Admin) |
| `repriceQuote` | `POST /api/v1/quotes/{id}/reprice?dryRun=` |
| `searchQuoteCatalog` | `GET /api/v1/quotes/catalog?search=&kind=Sewing\|Ppe` → models with active priced variants, PPE with prices |
| `getQuoteStaff` | `GET /api/v1/quotes/staff` (Director, Admin) |

Every command and query with input has a FluentValidation validator for lengths, ranges, paging and the price rules of R-08–R-10.

## Planned map (stage 1)

```text
src/Modules/Quotes/
  Quotes.Domain/Quotes.Domain/
    Quotes/Quote.cs                    aggregate: number, owner, items, save/version rules
    Quotes/QuoteItem.cs, QuoteItemOption.cs, QuotePrices.cs, QuoteNumber.cs
  Quotes.Application/Quotes.Application/
    Abstractions/                      catalog, customer, staff ports; IQuotesDbContext
    Access/QuoteAccess.cs              own vs all by role
    Quotes/Create|Save|Delete|Get|List|SetResponsible|Reprice/   command/query, handler, validator
    Catalog/SearchCatalog/             picker read
  Quotes.Infrastructure/Quotes.Infrastructure/
    Persistence/QuotesDbContext.cs, Configurations/, Migrations/
    DependencyInjection/
  Quotes.Api/Quotes.Api/QuotesController.cs, QuoteCatalogController.cs
src/Bootstrapper/Host.Api/Quotes/      Catalog, Crm, Identity adapters
tests/UnitTests/Quotes/, tests/IntegrationTests/Quotes/, ArchitectureTests (module isolation)
```

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | Backend module and Admin pages «КП» (list) and «Товари та ціни» (editor): R-01–R-12 | next |
| 2 | «Налаштування КП» and PDF: R-13, R-14 | planned |
| 3 | Gmail per manager and sending: R-15 | planned |
| 4 | КП для знайомства and templates: R-16, R-17 | planned |
| 5 | Composition editing with recalculation, model from scratch, history, archive, copy | planned |
| 6 | Old КП import (if feasible) and removal of the Crm quotes | planned |

Admin screens: brevi-web [QTE-001 screens](https://github.com/Weretik/brevi-web/blob/master/docs/specs/admin/quotes/001-quotes/README.md).

## Open questions

- Who may press «Передати розрахунок» besides Director and Admin: the old cabinet showed it to managers too. Until answered only Director and Admin assign (owner 2026-11-04).
- Exact PDF layout: from the old cabinet screenshots and, when reachable, the Calculator source on the owner's computer (`E:\Development\projects-brevi-reference\brevi-calculator`).
