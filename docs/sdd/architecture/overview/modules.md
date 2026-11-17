# Modules

Every module has Api, Application, Contracts, Domain, and Infrastructure projects; another module may use only its `Contracts` ([module rules](../../standards/module-rules.md)). Each section lists the module's aggregates: only a root carries `IAggregateRoot` and is written through a `DbSet` or repository, and its children change only through it ([DDD rules](../../standards/module-rules.md#domain-model-ddd)). The `Uses` and `offers` lines are what the `.csproj` references and `Contracts` projects hold today; `tools/docs/check_module_maps.py` fails CI when a module or project is missing here. How each link works is on the [C4 component diagram](../c4/README.md).

## ActivityLog

Layers: ActivityLog.Api, ActivityLog.Application, ActivityLog.Contracts, ActivityLog.Domain, ActivityLog.Infrastructure.

Aggregates (root → children): `ActivityEntry`.

Uses the `Contracts` of: —. `ActivityLog.Contracts` offers: `ActivityRecorded`, `IActivityDescriber`; it declares `IActivityActorNames`, which Identity implements ([ADR-0019](../decisions/0019-module-graph-without-cycles.md)).

Owns «Журнал дій» in schema `activity_log`: who created, changed or deleted what and when, for every staff member. It references no source module: Crm, Production, Warehouse, Catalog, Reference and Media raise domain events, describe them as `ActivityRecorded` of `ActivityLog.Contracts` and write them to their own outbox in the same save; ActivityLog's outbox handler stores each once. Staff names come from `Identity.Contracts`. Admin APIs live under `/api/activity-log` (`access:activity-log`, view only) ([ACT-001](../../specs/activity-log/001-staff-activity-log/README.md), [ADR-0016](../decisions/0016-domain-events-to-activity-log.md)).

## Identity

Layers: Identity.Api, Identity.Application, Identity.Contracts, Identity.Domain, Identity.Infrastructure.

Aggregates: none of its own; users and roles are ASP.NET Identity tables.

Uses the `Contracts` of: ActivityLog, Media. `Identity.Contracts` offers: `IStaffDirectory`, `StaffRoles`. Implements ActivityLog's `IActivityActorNames`.

Owns users, roles, refresh sessions, authentication, and access policies. The `api/auth/session` routes for login, refresh, logout, and the current user are implemented.

## Catalog

Layers: Catalog.Api, Catalog.Application, Catalog.Contracts, Catalog.Domain, Catalog.Infrastructure.

Aggregates (root → children): `Product` → `ProductCategoryReference`, `ProductCharacteristicTable`, `ProductInformationBlock`, `ProductPhoto`, `SewingArticle`, `SewingMaterialLine`, `SewingOperationLine`; `ProductCategory`.

Uses the `Contracts` of: ActivityLog, Media, Reference. `Catalog.Contracts` offers: `ICatalogProductReader`, `ISewingModelReader`, `ISewingPriceCalculator`. Implements Reference's `IPricingInputUsageSource`, `IPricingInputSubscriber`, `IPricingSettingsSubscriber` and Media's `IMediaFileUsageSource`.

Owns products and product categories. Product photos keep a Media library photo id and read URLs through `IProductMediaReader` (adapter over `Media.Contracts`); Catalog tells the library which products use a photo through the `IMediaFileUsageSource` port Media declares.

## Reference

Layers: Reference.Api, Reference.Application, Reference.Contracts, Reference.Domain, Reference.Infrastructure.

Aggregates (root → children): `Fabric`; `FabricColor`; `GarmentAccessory`; `GarmentPart`; `GarmentPartOperation`; `PpeMarkupRule`; `PricingParameter`; `ProductSection`; `Supplier`.

Uses the `Contracts` of: ActivityLog. `Reference.Contracts` offers: directories (`IFabricDirectory`, `IFabricColorDirectory`, `ISupplierDirectory`, `IPricingParameterDirectory` and others), `IPricingInputVersionReader`; it declares `IPricingInputUsageSource`, `IPricingInputSubscriber` and `IPricingSettingsSubscriber`, which Catalog implements.

Owns reference data for suppliers, fabrics, accessories, garment parts, operations on parts, and additional reference data.

## Accounting

Layers: Accounting.Api, Accounting.Application, Accounting.Contracts, Accounting.Domain, Accounting.Infrastructure.

Aggregates (root → children): `AuditEntry`; `BankStatement` → `BankStatementLine`; `FinanceAccount`; `FinanceAttachment`; `FinanceCategory`; `FinanceCounterparty`; `FinanceTransaction`; `FinanceTransfer`.

Uses the `Contracts` of: Crm, Reference. `Accounting.Contracts` offers: `IFinancePaymentReader`.

Owns «Бухгалтерія» in schema `accounting` with its own migration history: accounts and cash desks, income and expense categories and one change log (stage 1); operations, transfers, counterparties, attachments, bank import and reports follow by stage. Admin APIs live under `/api/accounting` for the Director only. Crm clients and Reference suppliers will be referenced by id without a database link ([ACC-001](../../specs/accounting/001-finance/README.md)).

## Crm

Layers: Crm.Api, Crm.Application, Crm.Contracts, Crm.Domain, Crm.Infrastructure.

Aggregates (root → children): `Customer` → `CustomerContact`, `CustomerExternalIdentity`; `EmbroiderySavedCalculation`; `EmbroiderySettings`; `Lead` → `LeadCall`; `LeadCallResult`; `LeadCategory`; `LeadSyncState`; `ManagerTask` → `ManagerTaskEvent`, `ManagerTaskStep`; `Quote` → `QuoteEvent`, `QuoteItem`; `QuoteIntroTemplate`; `QuoteSending`; `QuoteSettings`; `SiteRequest`; `StaffMailbox`.

Uses the `Contracts` of: ActivityLog, Catalog, Identity, Media, Reference, Reminders. `Crm.Contracts` offers: `ICustomerDirectory`, `IQuoteReader`, `IEmbroideryCalculationReader`; it declares `IQuoteOrderLinks`, which Production implements. Implements Media's `IMediaFileUsageSource`.

The client/contact slice implements scoped customer APIs, exact source identity, revision/idempotency and Admin merge in the existing four layers. See the [feature map](../../features/crm/clients-and-quotes.md). Crm is the «Менеджер сервісу» block: clients from KeepinCRM and КП ([QTE-001](../../specs/crm/003-quotes/README.md)); leads, deals and tasks will be added here.

## Media

Layers: Media.Api, Media.Application, Media.Contracts, Media.Domain, Media.Infrastructure.

Aggregates (root → children): `MediaFile`; `MediaFolder`.

Uses the `Contracts` of: ActivityLog. `Media.Contracts` offers: `IMediaFileReader`, `IMediaFileUploader`, `IMediaUrlImporter`, `ICdnStorage`; it declares `IMediaFileUsageSource`, which Catalog and Crm implement.

Owns the photo library in schema `media`: folders and photos on the Brevi CDN (adm.tools adapter in Infrastructure). Admin APIs live under `/api/media` for Manager and Admin. Other modules store only the photo id ([MED-001](../../specs/media/001-media-library/README.md)).

## Production

Layers: Production.Api, Production.Application, Production.Contracts, Production.Domain, Production.Infrastructure.

Aggregates (root → children): `ForecastQueue`; `ModelFabricNorm`; `PayrollAdjustment`; `PayrollIssued`; `PayrollMonth`; `PayrollPayment`; `PayrollRole`; `ProductionOrder` → `ProductionItem`, `ProductionOrderChange`, `ProductionOrderFile`, `ProductionShipment`; `ProductionTelegramDelivery`; `ProductionWork`; `WorkshopArchiveRecord`; `WorkshopEmployee`; `WorkshopSettings`; `WorkshopTimeChange`; `WorkshopTimeEntry`.

Uses the `Contracts` of: ActivityLog, Catalog, Crm, Identity, Media, Reference, Reminders. `Production.Contracts` offers: `IOrderFabricPlans`; it declares `IStockFacts`, which Warehouse implements. Implements Crm's `IQuoteOrderLinks`.

Owns the sewing workshop «Швейний цех» in schema `production`: workers (no sign-in), fabric norms per Catalog model code, orders, work log, payroll and reports. The fabric stock moved to the Warehouse module; Production gives it the plan of items by colour and reads issues and balances for «Звіти цеху» through its port `IProductionStockReader` over `Warehouse.Contracts`. Admin APIs live under `/api/production` for WorkshopChief and Admin. Catalog models and payroll coefficients from «Параметри ціноутворення» are read through ports adapted over `Catalog.Contracts` and `Reference.Contracts` ([PRD-001](../../specs/production/001-workshop-references/README.md)).

## Warehouse

Layers: Warehouse.Api, Warehouse.Application, Warehouse.Contracts, Warehouse.Domain, Warehouse.Infrastructure.

Aggregates (root → children): `FabricIssue` → `FabricIssueLine`; `FabricItemPlan` → `FabricItemPlanPart`; `FabricStock` (creates its `FabricMovement` records); `FabricMovement`; `FabricMovementRevision`; `StockOperation`; `StockTransfer`.

Uses the `Contracts` of: ActivityLog, Identity, Production, Reference. `Warehouse.Contracts` offers: nothing yet. Implements Production's `IStockFacts`.

Owns «Склад тканин» in schema `warehouse`: stock lines by fabric and colour, their movements and the «Видача» packages to order items. Admin APIs live under `/api/warehouse` (policies `access:fabric-stock`, and `access:production-orders` for the «Тканина» tab of an order item). Orders, items and their fabric plan stay in Production, fabrics, suppliers and colours in Reference; Warehouse keeps their ids as values and reads them through ports adapted over `Production.Contracts` and `Reference.Contracts` ([WH-001](../../specs/warehouse/001-warehouse-module/README.md)).

## Ai

Layers: Ai.Api, Ai.Application, Ai.Contracts, Ai.Domain, Ai.Infrastructure.

Aggregates (root → children): `AiPrompt`; `AiSettings`; `AiUsageMonth`.

Uses the `Contracts` of: —. `Ai.Contracts` offers: `IStructuredDataExtractor`, `IAiBasePrompt`.

Owns «Налаштування ШІ» in schema `ai`: the OpenAI key (encrypted with ASP.NET Data Protection, never returned), fast and strong models with their prices, on/off, the monthly limit and use, and the admin's extra rules per request kind. Admin APIs live under `/api/ai` (Admin). Other modules call it only through `Ai.Contracts` (`IStructuredDataExtractor`), implemented over `ExtractStructuredDataCommand` ([AI-001](../../specs/ai/001-ai-settings/README.md)).

## Purchasing

Layers: Purchasing.Api, Purchasing.Application, Purchasing.Contracts, Purchasing.Domain, Purchasing.Infrastructure.

Aggregates (root → children): `PurchaseCheckedDocument`; `PurchaseCheckedDocumentHistory`; `PurchaseMessage` → `PurchasePayment`; `PurchaseMessageFile`; `PurchaseRecognitionAttempt`; `PurchaseReviewChange`; `PurchaseSourceRevision`; `PurchaseSourceRow`; `PurchaseSyncState`.

Uses the `Contracts` of: Accounting, Ai, Reference, Scheduling. `Purchasing.Contracts` offers: nothing yet.

Owns the purchase invoice register «Рахунки на закупку» in schema `purchasing`: Telegram messages read from the Google Sheet «финансы», their sheet rows, raw history, document links, the manual supplier link and note, and the checked facts of each original (read by the Ai module or uploaded by hand). Admin APIs live under `/api/purchasing` for the Director only. Suppliers are referenced by Reference supplier id without a database link ([PUR-001](../../specs/purchasing/001-purchase-invoices/README.md)).

## Content

Layers: Content.Api, Content.Application, Content.Contracts, Content.Domain, Content.Infrastructure.

Aggregates (root → children): `Article` → `ArticleText`; `ProductPage` → `ProductPageText`; `SiteCategory` → `SiteCategoryText`; `SiteCategoryProduct`; `SiteCompanyInfo`; `SiteGallery`; `SitePage`; `SiteRedirect`; `SiteSeoSettings`.

Uses the `Contracts` of: Catalog, Media, Reference. `Content.Contracts` offers: nothing yet.

Owns what the public site says about a product in schema `content`: the «Показувати на сайті» mark, the page address and texts in Ukrainian and Russian (name, SEO, short facts, description, characteristics). It also owns the category tree of the site (categories and subcategories with their mark, address, order, SEO and text, and which products each holds) ([CNT-003](../../specs/content/003-site-categories/README.md)). Admin APIs live under `/api/content` (policies `access:content-products`, `access:content-categories` and `access:content-articles`). It also owns the articles of the site with their SEO; their pictures are photos of the Media library named by id and read through its port `IContentMediaReader` over `Media.Contracts` ([CNT-004](../../specs/content/004-articles/README.md)). The product itself, its section, fabrics, prices and photos stay in Catalog, Reference and Media; Content keeps only the Catalog product id without a database link and reads the rest through its port `IContentCatalogReader`, adapted over `Catalog.Contracts` and `Reference.Contracts` ([CNT-002](../../specs/content/002-product-pages/README.md)).

## Reminders

Layers: Reminders.Api, Reminders.Application, Reminders.Contracts, Reminders.Domain, Reminders.Infrastructure.

Aggregates (root → children): `NonWorkingDay`; `ReminderChat`; `ReminderOccurrence` → `ReminderAction`; `ReminderTask`; `SentReminderMessage`.

Uses the `Contracts` of: —. `Reminders.Contracts` offers: `ITelegramBot`, `TelegramGroup`.

Owns deadline reminder tasks, their periods and action history, Telegram chats and extra non-working days in schema `reminders`. Admin APIs live under `/api/reminders`; the Telegram webhook is in the same module, and the daily send runs as the `Host.Seed --reminders-dispatch=true` mode. See the [feature map](../../features/reminders/telegram-reminders.md).

## Scheduling

Layers: Scheduling.Api, Scheduling.Application, Scheduling.Contracts, Scheduling.Domain, Scheduling.Infrastructure.

Aggregates (root → children): `ScheduledTask`; `ScheduledTaskRun`.

Uses the `Contracts` of: —. `Scheduling.Contracts` offers: `IBackgroundTaskRequests` (start a «Фонові задачі» task, used by Purchasing for `purchase-recognition`).

Owns the background tasks «Фонові задачі» in schema `scheduling`: the code list of tasks, their schedules in Kyiv time, the scheduler tick that sends the outbox and starts the `brevierp-tasks` job for due tasks, the run history and «Запустити зараз». Admin APIs live under `/api/scheduling` ([SCH-001](../../specs/scheduling/001-background-tasks/README.md)).

## Shared BuildingBlocks

Shared domain primitives, Application behaviors, domain event and result handling, and technical adapters.
