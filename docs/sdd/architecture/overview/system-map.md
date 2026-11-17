# System map

The first page to open for "where does this live": every host and every module with one line of what it owns. The [module map](modules.md) gives each module's projects, which `Contracts` it uses and what its own `Contracts` offer; the [feature map](../../features/README.md) goes from a module to its features and code. `tools/docs/check_module_maps.py` runs in CI and fails when a module, host, endpoint folder or feature card is missing from these maps, so a change that adds or renames one updates them in the same pull request ([delivery rules](../../standards/delivery-rules.md)).

## Executable hosts

- `src/Bootstrapper/Host.Api` composes DI, middleware, the modules' Minimal API endpoints, authentication, and OpenAPI.
- `src/Bootstrapper/Host.Seed` is a console host for approved seed operations and the Cloud Run Job `brevierp-tasks` that the scheduler tick starts for due background tasks.
- `src/Jobs/CatalogImport` and `src/Jobs/SewingPricing` are operational job libraries (catalogue import, pricing verification and activation). Only `Host.Seed` references and runs them; Domain, Application, and `Host.Api` do not. See the [jobs overview](../../../../src/Jobs/README.md).

## Business modules

- Catalog owns products, categories, catalog photos, Sewing Articles and composition, and Sewing/PPE pricing.
- Reference owns supplier, fabric, accessory, garment-part, operation and pricing-parameter reference data.
- Crm owns clients and contacts from KeepinCRM, КП, embroidery prices, manager tasks, site requests, and leads.
- Content owns what the public site says: product pages, site categories, articles, redirects, site pages, SEO settings, company facts, and site galleries.
- Production owns the sewing workshop «Швейний цех»: workers, fabric norms, production orders, work log, timesheet, payroll, reports, and the workshop archive.
- Warehouse owns the fabric stock «Склад тканин»: stock lines, movements, and issues to order items.
- Accounting owns «Бухгалтерія»: accounts and cash desks, categories, operations, transfers, counterparties, statements, and reports.
- Purchasing owns the purchase invoice register «Рахунки на закупку».
- Media owns the photo library on the Brevi CDN.
- Ai owns «Налаштування ШІ» and structured data extraction through OpenAI.
- Reminders owns deadline reminders and their Telegram chats.
- ActivityLog owns «Журнал дій»: who created, changed or deleted what.
- Scheduling owns the background task schedules, the scheduler tick, the outbox run, and the run history.
- Identity owns ASP.NET Identity, roles, «Права доступу» policies, current user/session integration, and seed data.
- BuildingBlocks owns shared abstractions and cross-cutting implementations; it must not acquire business rules.

## Context map

The context map is the [C4 component diagram](../c4/README.md#level-3-components-of-the-backend): every module and outside system, and on every arrow how they talk. A `Contracts` interface is a synchronous call inside one process; the outbox is a message sent after the commit and repeated until it succeeds; an adapter in `Infrastructure/Integrations` translates an outside system into the module's own model (anti-corruption layer). Five module pairs still reference each other both ways; they are open in the [module rules](../../standards/module-rules.md#known-deviations).

## Dependency direction

```text
Host.Api / Host.Seed ──→ Jobs.* (Host.Seed only)
        │
        ├── Module.Api ──→ Module.Application ──→ Module.Domain
        └── Module.Infrastructure ──────────────→ Module.Application + Module.Domain

BuildingBlocks.* supplies shared primitives; it does not depend on business modules.
```

Domain does not depend directly on Application or Infrastructure, and Application does not depend on a concrete DbContext, HTTP client, scheduler, or EF repository.
