# QTE-001 — «КП»: commercial proposals

**Status:** stages 1–4 implemented 2026-11-04. **Date:** 2026-11-03 (idea), 2026-11-04 (specification, stage 1). **Module:** Crm («Менеджер сервісу»: clients, КП, later leads, deals and tasks), schema `crm`, migration history `crm.__EFMigrationsHistory` ([module rules](../../../standards/module-rules.md)). Feature card: [clients and КП](../../../features/crm/clients-and-quotes.md).

## Why

Managers build commercial proposals (КП) in the client's old Calculator cabinet. Brevi now holds everything a КП is built from: products with fabric variants and three published prices, PPE with live prices, short «для КП» descriptions of products and fabrics, current site links, photos and clients from KeepinCRM. The product owner asked on 2026-11-04 for the first working ways to create a КП in the Admin, with the screens and rules of the old cabinet (owner's screenshots and answers in the project thread «Сделать создание КП в Brevi»).

## Decision: new КП inside Crm, the earlier Crm quotes removed

Crm contained versioned quotes, introductions, issuer settings, PDF, Gmail delivery and archive tooling built by Codex during the earlier port ([CRM-001](../001-clients-quotes-custom-sewing/README.md), history only). It was written against the old catalogue, was never used by staff, and was about 700 files of journals and idempotency stores that a КП does not need.

The owner decided on 2026-11-04: delete all of that work, keep the clients from KeepinCRM, and build the new КП in the Crm module, which is the whole «Менеджер сервісу» block (КП now; leads, deals and tasks later), with its own schema and clean architecture. Migration `RemoveCodexQuoteTables` drops the old tables and their trigger functions (their test data is not kept); `AddQuotes` creates the new ones.

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

Stage 2 design: one `crm.QuoteSettings` row (sections as jsonb, `Version` concurrency token); before the first save the API returns the old Calculator values (`QuoteSettingsDefaults`). A staff member without a saved contact shows their account name, phone and email. The PDF is drawn on request by QuestPDF (Community license) with embedded Noto Sans (`GET /api/crm/quotes/{id}/pdf`), nothing is stored; photos and the logo are read from their https links, a broken link is left out. The intro texts and letter variants are stored now and used in stage 4.

### Stage 3 — sending by Gmail

- **R-15:** Each manager sends from their own Gmail address, connected once in «Налаштування КП» (the Director presses «Підключити» for a manager, the manager grants Google send access once; the Google OAuth app the owner already created for the earlier single sender is reused). Sending shows the recipient, subject and text (editable), attaches the PDF of the saved КП and on success marks the КП sent with the date. A failure leaves it a draft and shows the reason.

Stage 3 design: Google OAuth web client with `openid email gmail.send` only (no reading of mail). «Підключити Gmail» (`POST /api/crm/mailboxes/{userId}/connect`, the Director or Admin for anyone, a manager for themselves) returns the consent address with a signed 15-minute state; Google calls `GET /api/crm/mailboxes/google-callback`, which stores the verified address and the refresh token encrypted by Data Protection in `crm.StaffMailboxes` and redirects to the Admin. A manager sends from their own connected Gmail, not the responsible person's: `GET …/send-draft` gives the letters (template first, then variants) with marks filled from the sender's КП contact; `POST …/send` with the shown version builds the PDF exactly as R-14, sends it through the Gmail upload endpoint and only then records `crm.QuoteSendings`, sets Sent and `LastSentAt`. A sent КП can be edited and sent again; only drafts can be deleted. Without the Google client id and secret Gmail is off and both actions say so ([setup](../../../operations/configuration/crm-gmail.md)). An empty saved phone or email of a КП contact now falls back to the account.

### Stage 4 — introduction КП

- **R-16:** «КП для знайомства» is a КП of kind Introduction with a layout: «Загальне» (page «Літній спецодяг» and page «Зимовий спецодяг»), «Літній» or «Зимовий» (one page). Each page starts with the fixed main model of its season («Імпульс» for summer, «Прайм» for winter; their product codes are set in «Налаштування КП», defaults `MOD-166` and `MOD-169`) and has three more distinct active sewing models of that season (Catalog season «Літо» or «Зима»). Every model shows one fabric variant with its published від 41 шт. price; the other prices are hidden and nothing is typed by hand.
- **R-17:** The introduction has an internal name (as R-04), a recipient email (optional on save, up to 320), an optional contact person and company name, an internal note and the chosen letter variant from «Налаштування КП». The PDF is a promotional catalogue: per page a banner with the season title and subtitle, the logo text, the models with photo, fabric, від 41 шт. price and the product button, the assortment link; then the five «Переваги», the next step text and the manager's contact; requisites in the footer.
- **R-18:** Sending an introduction offers the letter variants (the chosen one first) with marks filled from the sender's contact, and fills «Кому» with the recipient email. Everything else follows R-15.
- **R-19:** Any Manager, Director or Admin saves the current layout, models, fabrics and letter variant as a named «Готовий шаблон» (name up to 100, unique ignoring case). All staff see all templates; the author, Director or Admin delete one. Applying a template fills the selection again with current Catalog data and prices; a model or fabric that is no longer active is left empty with the reason.
- **R-20:** «Виріб з нуля» (owner 2026-11-04): an item of a КП «під запит» that is not in the Catalog. The manager names it, picks accessories and operations from the references (shared, with quantities; minutes for an operation without a norm) and one or more fabrics with metres; each fabric is a price row. Prices come from the Catalog sewing formula with the current settings and reference versions, never typed; they stay while the composition and fabrics stay (R-11), change when they change, and «Перерахувати» prices the saved composition again. The item is not added to the Catalog. Started from «Нове КП → КП з нуля» or «Додати виріб з нуля» in the products block.

Stage 4 design: `Quotes` gains `IntroLayout`, `RecipientEmail`, `LetterName` (null for a request КП). An introduction is saved by its own endpoints with pages of models (`productId`, `articleId`, optional saved `itemId`); the server turns them into ordinary sewing items with one option each (only від 41 шт. shown) through `QuoteItemsBuilder`, so saved prices stay until «Перерахувати» (R-11), and then checks the layout, seasons, main models and repeats. `crm.QuoteIntroTemplates` keeps a template as jsonb pages of `productId`/`articleId` with the product and fabric names for display. The editor reads the main models and letter names from `GET /api/crm/quote-lookups/introduction` (managers cannot read the settings) and season models from `searchCrmQuoteCatalog` with `season`. The PDF composer draws the catalogue for Introduction and the item table for Request.

Stage 5a design: a «Виріб з нуля» is a `QuoteItems` row of kind `CustomSewing` without `ProductId`; its accessories and operations are `crm.QuoteItemLines` (reference id, copied name and unit, quantity, manual minutes) and each fabric is an option with `FabricId` and `FabricMeters`. Crm prices it through the port `IQuoteSewingCalculator`; the host adapter sends the Catalog query `CalculateSewingCompositionQuery` once per fabric (the article evaluator and this query share `SewingPricingContext`: settings candidates, reference versions, blockers) and turns blockers into short reasons. Saving compares lines and fabrics with the saved item: unchanged keeps prices, changed prices again.

### Later stages

Editing the composition of a sewing model inside a КП with recalculation and the seamstress wage («Розцінка швачки»), a model from scratch, change history, archive and copy, export for production, import of old Calculator КП (only if the partial export allows it; otherwise start clean, owner 2026-11-04).

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

Schema `crm`, see [Crm schema](../../../architecture/data-model/crm.md). Catalog, Reference and Identity ids are stored as values, without foreign keys; `CustomerId` points to a Crm customer without a foreign key, and the KeepinCRM sync keeps customers that a КП uses.

| Table | Key columns |
| --- | --- |
| `Quotes` | `Id` (bigint identity), `Number` (unique), `NumberYear`, `NumberMonth`, `NumberSequence` (unique triple), `Kind`, `Status`, `InternalName`, `CustomerId`, `RecipientName`, `CompanyName`, `ClientComment`, `ManagerNote`, `AuthorUserId`, `AuthorName`, `ResponsibleUserId`, `ResponsibleName`, `CreatedAt`, `UpdatedAt`, `Version` (concurrency) |
| `QuoteItems` | `Id`, `QuoteId` (cascade), `Position`, `Kind` (`Sewing`, `Ppe`, `OneOffPpe`, `CustomSewing`), `ProductId` (Catalog; null for one-off PPE and «Виріб з нуля»), `Code`, `Name`, `Subtitle`, `Description`, `SiteUrl`, `PhotoUrl`, `Season`, `SupplierUrl` |
| `QuoteItemOptions` | `Id`, `ItemId` (cascade), `Position`, `ArticleId` (Catalog), `FabricId` (Reference), `Title`, `Details`, `IsRecommended`, for each tier (`Retail`, `Wholesale`, `Large`): `<Tier>Source`, `<Tier>Manual`, `<Tier>Shown`, plus `ManualReason`, `ManualByUserId`, `ManualByName`, `ManualAt`, `PricesTakenAt`, `FabricMeters` (R-20) |
| `QuoteItemLines` | `Id`, `ItemId` (cascade), `Position`, `Kind` (`Accessory`, `Operation`), `ReferenceId`, `Name`, `Unit`, `Quantity`, `ManualMinutes` (R-20) |

Money is `numeric(12,2)` as Catalog publishes it. A save replaces the item rows (new ids in the response). Rollback: the previous image keeps working with the new tables unused; the dropped Codex tables are not restored with data.

## Reads from other modules (adapters in Host.Api)

| Port (Crm.Application) | Source | Used for |
| --- | --- | --- |
| `IQuoteCatalogReader` (`Host.Api/Crm/QuoteCatalogReader.cs`) | Catalog «Товари» overview (published variant prices, PPE rule prices, problems), product admin detail (main visible photo), Reference fabrics (`ProposalDescription`, composition, density, price, supplier) | Picker search, adding and repricing items |
| `IQuoteStaffReader` (`Host.Api/Crm/QuoteStaffReader.cs`) | Identity users with Manager or Admin role, not blocked | Responsible person list, actor names |

Clients come from the same module (`/api/crm/customers`).

## API (stage 1)

Contract: [crm/quotes.openapi.yaml](../../../contracts/crm/quotes.openapi.yaml). Every operation needs a signed-in Manager or Admin (the Director also holds Admin); ownership is checked in the handlers.

| Operation | Endpoint |
| --- | --- |
| `getCrmQuotes` | `GET /api/crm/quotes?search=&status=&responsibleUserId=&page=&pageSize=` (50 per page, max 100, newest change first) |
| `getCrmQuote` | `GET /api/crm/quotes/{id}` |
| `createCrmQuote` | `POST /api/crm/quotes` → 201 with the number |
| `saveCrmQuote` | `PUT /api/crm/quotes/{id}` with `version`; the whole editable content |
| `deleteCrmQuote` | `DELETE /api/crm/quotes/{id}?version=` (draft only) |
| `setCrmQuoteResponsible` | `PUT /api/crm/quotes/{id}/responsible` (Admin) |
| `repriceCrmQuote` | `POST /api/crm/quotes/{id}/reprice` with `version`, `apply` |
| `searchCrmQuoteCatalog` | `GET /api/crm/quote-lookups/catalog?kind=Sewing\|Ppe&season=&search=&page=&pageSize=` |
| `getCrmQuoteStaff` | `GET /api/crm/quote-lookups/staff` (Admin) |
| `getCrmQuoteSendDraft` | `GET /api/crm/quotes/{id}/send-draft` (stage 3) |
| `sendCrmQuote` | `POST /api/crm/quotes/{id}/send` with `version`, `to`, `subject`, `body`; 409 with the reason on failure |
| `connectCrmMailbox` / `disconnectCrmMailbox` | `POST /api/crm/mailboxes/{userId}/connect` with `returnPath`, `DELETE /api/crm/mailboxes/{userId}` |
| `createCrmQuoteIntroduction` / `saveCrmQuoteIntroduction` | `POST /api/crm/quote-introductions`, `PUT /api/crm/quote-introductions/{id}` with `version` (stage 4); `getCrmQuote` returns the `introduction` block |
| `getCrmQuoteIntroductionSetup` | `GET /api/crm/quote-lookups/introduction`: main models with fabrics, letter variant names |
| `getCrmQuoteCatalogProducts` | `GET /api/crm/quote-lookups/products?ids=` (up to 20): current data of chosen models |
| `calculateCrmQuoteCustomSewing` | `POST /api/crm/quote-lookups/custom-sewing-price`: «Виріб з нуля» price per fabric, line names, minutes and labour, or problems; nothing saved (R-20) |
| `getCrmQuoteIntroTemplates` / `createCrmQuoteIntroTemplate` / `deleteCrmQuoteIntroTemplate` | `GET`, `POST /api/crm/quote-intro-templates`, `DELETE /api/crm/quote-intro-templates/{id}` |

Every command and query with input has a FluentValidation validator (lengths, ranges, paging, item kinds, no manual sewing price, one row for PPE, one recommended variant); Catalog-dependent rules (active product, published price, manual reason) are checked by `QuoteItemsBuilder` and return 409 or 400 with a Ukrainian message.

## Map (stage 1, actual)

The file map is in the [feature card](../../../features/crm/clients-and-quotes.md#кп-commercial-proposals). Tests: `tests/UnitTests/Crm/Quotes/`, `tests/IntegrationTests/Crm/Quote*Tests.cs`.

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | Backend in Crm and Admin pages «КП» (list) and «Товари та ціни» (editor): R-01–R-12 | done 2026-11-04 |
| 2 | «Налаштування КП» and PDF: R-13, R-14 | done 2026-11-04 |
| 3 | Gmail per manager and sending: R-15 | done 2026-11-04 |
| 4 | КП для знайомства and templates: R-16–R-19 | done 2026-11-04 |
| 5a | «Виріб з нуля»: R-20 | done 2026-11-04 |
| 5b | Changing the composition of a Catalog model in a КП, history, archive, copy | planned |
| 6 | Old КП import (if feasible) | planned |

Admin screens: brevi-web [QTE-001 screens](https://github.com/Weretik/brevi-web/blob/master/docs/specs/admin/quotes/001-quotes/README.md).

## Open questions

- Who may press «Передати розрахунок» besides Director and Admin: the old cabinet showed it to managers too. Until answered only Director and Admin assign (owner 2026-11-04).
- Exact PDF layout: no sample of the old PDF was available; stage 2 uses its own layout (header with requisites, items with photo and shown prices, comment, VAT note, call-to-action, requisites footer). Adjust when the owner sends a sample.
