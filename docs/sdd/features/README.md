# BreviERP backend feature map

**Static code check completed:** 2026-10-16. This map covers current HTTP scenarios by module. Each description explains the entry point, components, data, and cross-module relationships; [SDD](../specs/README.md) holds decisions and delivery history. When adding a feature or changing its structure, update its description and this index in the same change.

See the feature card's “Code structure” section for actual file paths and roles, “Structure diagram” for the flow between parts, and “Tests” for test files and the focused run command.

## Catalog

Products:

- [Catalog products](catalog/products.md) — create, read, replace, and delete Sewing/PPE products; the «Товари» page overview (section, season, check, current site link, quote description). Planned modules: [«Контент сайту»](../specs/content/001-site-content/README.md).
- [Product categories](catalog/categories.md) — Admin CRUD and Storefront list.
- [Sewing Articles and composition](catalog/sewing-composition.md) — sale SKUs, shared/Article material norms, shared operations and review readiness.

Pricing API (start with Sewing or PPE pricing):

- [Sewing Article pricing](catalog/sewing-pricing.md) — versioned inputs, strict calculation, saved snapshots/publication API and private custom arithmetic adapter.
- [PPE pricing](catalog/ppe-pricing.md) — versioned supplier offers, exact rule scope and three-tier calculation.
- [PPE supplier price import](catalog/ppe-supplier-prices.md) — «Імпорт цін»: supplier sources, discounts, Insight XML and СЕМТО PDF uploads with preview, Monday 09:00 АВ-Центр/3C-Маркет/Перчатка-центр/Трєле cabinet updates, «Оновити зараз».
- [Current pricing boundaries](catalog/pricing-cutover.md) — Product reads carry metadata only; prices come from the pricing APIs.
- [Reference pricing versions](reference/pricing-input-versions.md) — Reference-owned cost and norm versions used by pricing.

Pricing data operations (local `Host.Seed` jobs, not HTTP), in run order:

- [Catalogue import](catalog/catalog-import.md) — frozen source, transactional model/Article/PPE transfer, provenance, reconciliation and dedicated cloud job with target/source pins.
- [Imported pricing readiness](catalog/pricing-readiness.md) — full strict inventory before review/activation.
- [Frozen-source pricing input review](catalog/pricing-input-review.md) — per-field source checks and activation holds.
- [Explicit local pricing activation](catalog/pricing-activation.md) — reviewed plan, confirmation/publication and progress.

## Reference

- [Suppliers](reference/suppliers.md) — CRUD, list reads and KeepinCRM refresh.
- [Fabrics](reference/fabrics.md) — CRUD, list reads and import of the Calculator sheet «Тканини».
- [Garment accessories](reference/garment-accessories.md) — CRUD, list reads and import of the Calculator sheet «Фурнітура».
- [Garment parts](reference/garment-parts.md) — CRUD and list reads.
- [Sewing operations](reference/garment-part-operations.md) — CRUD, list reads, and import of the Calculator sheet «Операції».
- [Reference pricing versions](reference/pricing-input-versions.md) — atomic numeric-edit history and explicit service-cost revisions (also listed under Catalog pricing).
- [Pricing parameters](reference/pricing-parameters.md) — «Параметри ціноутворення»: sheet «Налаштування цін» (parameters + PPE markup rules), CRUD and one-time sheet import.
- [Product sections](reference/product-sections.md) — «Типи і розділи»: sewing and PPE sections, CRUD and one-time sheet import.

## Identity

- [User session](identity/session.md) — Admin sign-in with the session cookie, CSRF, lockout, first administrator.
- [Staff accounts](identity/staff-accounts.md) — «Користувачі»: administrators and the director create, re-role, block, reset and delete staff accounts.
- [Own profile](identity/own-profile.md) — «Мій кабінет»: own name and photo for every staff member; phone, email and password for administrators.

## Other modules

The [module map](../architecture/overview/modules.md) describes Accounting/Crm boundaries; implemented Crm workflows are listed below.

## Crm

- [Clients and КП](crm/clients-and-quotes.md) — clients and contacts from KeepinCRM; КП (commercial proposals) stages 1–6: create, «Товари та ціни», reprice, give to a manager, settings, PDF, Gmail, introductions, «Виріб з нуля», history, archive, copy, old КП transfer.
- [Manager tasks](crm/manager-tasks.md) — Director and Admin give tasks to managers and track workload; managers move them on the «Мої завдання» kanban.

## Media

- [Photo library «Фото»](media/library.md) — folders and subfolders on the Brevi CDN, upload, search by name or folder, move and delete; products keep the photo id.

## Production

- [Workshop references «Швейний цех»](production/workshop-references.md) — workers of the workshop, fabric norms per sewing model, payroll coefficients from «Параметри ціноутворення» (stage 1 of the «Цех» port).

## Reminders

- [Telegram reminders](reminders/telegram-reminders.md) — deadline tasks, working-day Telegram cards at 09:00 Kyiv with «Виконано» / «Нагадати пізніше», period history, chats and extra days off.

## Platform access

- API address: the [Cloudflare API proxy](../operations/deployment/cloudflare-api-proxy.md) serves `api.brevi.com.ua`; only sign-in, the Telegram webhook and public storefront categories are anonymous ([roles and policies](../architecture/security/authorization/roles-and-policies.md#current-anonymous-exceptions)).
