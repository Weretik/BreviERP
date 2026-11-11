# Modules

## Identity module

Owns users, roles, refresh sessions, authentication, and access policies. The `api/auth/session` routes for login, refresh, logout, and the current user are implemented.

## Catalog module

Owns products and product categories. Product photos keep a Media library photo id and read URLs through `IProductMediaReader` (host adapter); Catalog answers which products use a photo through `GetProductMediaUsagesQuery`.

## Reference module

Owns reference data for suppliers, fabrics, accessories, garment parts, operations on parts, and additional reference data.

## Accounting module

Owns «Бухгалтерія» in schema `accounting` with its own migration history: accounts and cash desks, income and expense categories and one change log (stage 1); operations, transfers, counterparties, attachments, bank import and reports follow by stage. Admin APIs live under `/api/accounting` for the Director only. Crm clients and Reference suppliers will be referenced by id without a database link ([ACC-001](../../specs/accounting/001-finance/README.md)).

## Crm module

The client/contact slice implements scoped customer APIs, exact source identity, revision/idempotency and Admin merge in the existing four layers. See the [feature map](../../features/crm/clients-and-quotes.md). Crm is the «Менеджер сервісу» block: clients from KeepinCRM and КП ([QTE-001](../../specs/crm/003-quotes/README.md)); leads, deals and tasks will be added here.

## Media module

Owns the photo library in schema `media`: folders and photos on the Brevi CDN (adm.tools adapter in Infrastructure). Admin APIs live under `/api/media` for Manager and Admin. Other modules store only the photo id ([MED-001](../../specs/media/001-media-library/README.md)).

## Production module

Owns the sewing workshop «Швейний цех» in schema `production`: workers (no sign-in), fabric norms per Catalog model code, orders, work log, payroll and reports. The fabric stock moved to the Warehouse module; Production gives it the plan of items by colour and reads issues and balances for «Звіти цеху» through the host port `IProductionStockReader`. Admin APIs live under `/api/production` for WorkshopChief and Admin. Catalog models and payroll coefficients from «Параметри ціноутворення» are read through host ports ([PRD-001](../../specs/production/001-workshop-references/README.md)).

## Warehouse module

Owns «Склад тканин» in schema `warehouse`: stock lines by fabric and colour, their movements and the «Видача» packages to order items. Admin APIs live under `/api/warehouse` (policies `access:fabric-stock`, and `access:production-orders` for the «Тканина» tab of an order item). Orders, items and their fabric plan stay in Production, fabrics, suppliers and colours in Reference; Warehouse keeps their ids as values and reads them through host ports ([WH-001](../../specs/warehouse/001-warehouse-module/README.md)).

## Ai module

Owns «Налаштування ШІ» in schema `ai`: the OpenAI key (encrypted with ASP.NET Data Protection, never returned), fast and strong models with their prices, on/off, the monthly limit and use, and the admin's extra rules per request kind. Admin APIs live under `/api/ai` (Admin). Other modules call it only through ports implemented in the host, which send `ExtractStructuredDataCommand` ([AI-001](../../specs/ai/001-ai-settings/README.md)).

## Purchasing module

Owns the purchase invoice register «Рахунки на закупку» in schema `purchasing`: Telegram messages read from the Google Sheet «финансы», their sheet rows, raw history, document links, the manual supplier link and note, and the checked facts of each original (read by the Ai module or uploaded by hand). Admin APIs live under `/api/purchasing` for the Director only. Suppliers are referenced by Reference supplier id without a database link ([PUR-001](../../specs/purchasing/001-purchase-invoices/README.md)).

## Content module

Owns what the public site says about a product in schema `content`: the «Показувати на сайті» mark, the page address and texts in Ukrainian and Russian (name, SEO, short facts, description, characteristics). It also owns the category tree of the site (categories and subcategories with their mark, address, order, SEO and text, and which products each holds) ([CNT-003](../../specs/content/003-site-categories/README.md)). Admin APIs live under `/api/content` (policies `access:content-products`, `access:content-categories` and `access:content-articles`). It also owns the articles of the site with their SEO; their pictures are photos of the Media library named by id and read through the host port `IContentMediaReader` ([CNT-004](../../specs/content/004-articles/README.md)). The product itself, its section, fabrics, prices and photos stay in Catalog, Reference and Media; Content keeps only the Catalog product id without a database link and reads the rest through the host port `IContentCatalogReader` ([CNT-002](../../specs/content/002-product-pages/README.md)).

## Reminders module

Owns deadline reminder tasks, their periods and action history, Telegram chats and extra non-working days in schema `reminders`. Admin APIs live under `/api/reminders`; the Telegram webhook is in the same module, and the daily send runs as the `Host.Seed --reminders-dispatch=true` mode. See the [feature map](../../features/reminders/telegram-reminders.md).

## Shared BuildingBlocks

Shared domain primitives, Application behaviors, domain event and result handling, and technical adapters.
