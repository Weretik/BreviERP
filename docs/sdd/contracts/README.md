# API Contracts

`docs/sdd/contracts/` is the canonical, versioned source for machine-readable public API contracts.

## Structure

```text
docs/sdd/contracts/
├── openapi.yaml                         # aggregate entry point for all public APIs
└── <module>/
    └── <feature>.openapi.yaml           # contract owned by one feature
```

`openapi.yaml` is the entry point for Swagger UI, frontend client generation, and contract tests. It references module contracts through `$ref`; do not duplicate operations or schemas in the aggregate file.

## Current frontend handoff

- [Frontend integration guide](frontend-integration.md) describes authentication, CSRF, pagination, errors, and current contract coverage.
- [Aggregated OpenAPI 3.1 contract](openapi.yaml) covers APIs for which a versioned contract has been created and verified.
- Identity: [session](identity/session.openapi.yaml), [access rights](identity/access.openapi.yaml).
- Catalog: [products](catalog/product-catalog.openapi.yaml), [«Товари» page overview](catalog/product-overview.openapi.yaml), [categories](catalog/product-dependencies.openapi.yaml).
- Media: [photo library with folders](media/media-library.openapi.yaml).
- Production: [workshop references](production/workshop-references.openapi.yaml), [orders](production/production-orders.openapi.yaml).
- Warehouse: [fabric stock](warehouse/fabric-stock.openapi.yaml).
- Reference: [suppliers](reference/suppliers.openapi.yaml), [fabrics](reference/fabrics.openapi.yaml), [garment accessories](reference/garment-accessories.openapi.yaml), [garment parts](reference/garment-parts.openapi.yaml), [operations](reference/garment-part-operations.openapi.yaml), [pricing parameters and PPE markup rules](reference/pricing-parameters.openapi.yaml), [product sections](reference/product-sections.openapi.yaml), [fabric colours](reference/fabric-colors.openapi.yaml).

OpenAPI files describe the contract of the actual code. They do not make an endpoint publicly accessible: the host endpoint conventions, authorization attributes and fallback policy determine runtime access.

Each feature SDD contains `contracts/api-contract.md` for human-readable decisions: boundaries, consumers, compatibility, security, errors, idempotency, and deployment. For new or changed HTTP behavior, agree on these decisions and create or update the machine-readable contract before implementing the transport layer where practical. Add focused Red contract/API tests based on the agreed contract, then implement the transport. The feature document must link to `docs/sdd/contracts/<module>/<feature>.openapi.yaml`, and the aggregate `openapi.yaml` must be updated in the same change.

Do not create a public OpenAPI document for internal-only changes. Before adding an HTTP contract, agree on the route, method, operation ID, DTOs, examples, errors, security, idempotency for side-effecting writes, and migration path for breaking changes.

- Crm: [clients and contacts](crm/clients-quotes.openapi.yaml), [КП (commercial proposals)](crm/quotes.openapi.yaml), [manager tasks](crm/tasks.openapi.yaml), [leads](crm/leads.openapi.yaml), [embroidery price](crm/embroidery.openapi.yaml).

- Ai: [«Налаштування ШІ»](ai/ai-settings.openapi.yaml) — the OpenAI key (write-only), models, prices, limit and extra rules.
- Accounting: [«Бухгалтерія»](accounting/finance.openapi.yaml) — «Рахунки і каси» with balances, «Категорії», «Операції», «Перекази», «Огляд», «Контрагенти», «Потребують перевірки» and their change log; director only.
- Content: [«Контент → Товари» site pages of products](content/content-products.openapi.yaml) — the «Показувати на сайті» mark, address and texts in two languages.
- Content: [«Контент → Розділи» categories of the site](content/content-categories.openapi.yaml) — the two-level tree, the «Показувати на сайті» mark, address, order and texts in two languages.
- Content: [«Контент → Статті» articles of the site](content/content-articles.openapi.yaml) — the «Показувати на сайті» mark, address, date, cover and texts with SEO in two languages; pictures are photos of the «Фото» library; one-time transfer of the old-site articles with their pictures.
- Content: [«Контент → Переадресації» redirects of the site](content/content-redirects.openapi.yaml) — 301 redirects: list, add, change, delete and the transfer of the old-site addresses.
- Content: [public redirects of the site](content/content-public-redirects.openapi.yaml) — anonymous list of every redirect with the final page of its chain.
- Content: [public articles of the site](content/content-public-articles.openapi.yaml) — anonymous read-only list and page of the articles marked «Показувати на сайті», with SEO, text and CDN pictures; Ukrainian when there is no Russian text.
- Content: [public catalog of the site](content/content-public-catalog.openapi.yaml) — anonymous read-only tree, site categories with SEO and text, product cards with «від» (also by category), product by address, addresses for the sitemap.
- Purchasing: [«Рахунки на закупку» register](purchasing/purchase-invoices.openapi.yaml) — purchase invoices read from the Google Sheet «финансы».

- Reminders: [Telegram reminders admin API](reminders/telegram-reminders.openapi.yaml) — tasks, periods, chats, non-working days and bot connection. The Telegram webhook is intentionally not in the contract.

## Access

Every operation requires a signed-in Admin session cookie (`__Host-brevi.session`) unless the roles and policies (`docs/sdd/architecture/security/authorization/roles-and-policies.md`) list it as public. Changing requests also send `X-CSRF-Token`. The temporary anonymous mode (`ApiAccess:AnonymousEnabled`) is removed.
