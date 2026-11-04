# Production schema

Code model reviewed 2026-11-04. `Production.Infrastructure.Persistence.ProductionDbContext` owns schema `production`; migration history is `production.__EFMigrationsHistory`. Migration `InitialProduction` creates Employees and FabricNorms ([PRD-001](../../specs/production/001-workshop-references/README.md)); `ProductionOrders` creates Orders, Items and OrderChanges ([PRD-002](../../specs/production/002-production-orders/README.md)); `ProductionTracking` adds Shipments, OrderFiles, TelegramDeliveries, Settings and the stage and cancel columns ([PRD-003](../../specs/production/003-order-tracking/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| Employees | Generated int id. FullName (120), NameKey (120, lower case), Position (80), IsActive, CreatedAt, UpdatedAt, UpdatedByUserId (Identity user id as a value). Unique NameKey among active rows (filtered index `"IsActive"`) |
| FabricNorms | Generated int id. Unique ModelCode (32, Catalog model code as a value), OneColor, TwoColorsMain, TwoColorsAdditional, ThreeColorsMain, ThreeColorsAdditional1, ThreeColorsAdditional2 (numeric(7,3) metres, nullable), UpdatedAt, UpdatedByUserId |
| Orders | Generated int id. Unique Number, QuoteId (Crm КП id as a value), QuoteNumber (40), CustomerId, ClientName (300), ManagerUserId, ManagerName (200), Status (New/Closed/Cancelled), OrderDate, Comment, PriceTier (Retail/Wholesale/Large), unique CreateRequestId and LegacyId (filtered), CopiedFromOrderId, CreatedAt, CreatedByName, UpdatedAt, CancelReason (1000), DeletedAt (soft delete), Version (concurrency) |
| Items | Generated int id, OrderId → Orders (cascade), unique (OrderId, Position). Snapshot: QuoteItemId, QuoteOptionId, ProductId, ArticleId, ModelCode, ModelName, FabricId, FabricCode, FabricName, SewingRate, ReportUnitPrice (numeric(12,2)). Details: Quantity, DueDate, MainColor, AdditionalColors (jsonb list), SizesText, TechnicalChanges, Comment, Applications (jsonb list of kind/place/description/color), PlannedFabricMeters. Stages (jsonb list of stage numbers 1–9), CurrentStage (int, indexed: the furthest one) |
| Shipments | Generated int id, OrderId → Orders (cascade), ItemId → Items (restrict). Quantity, ShippedOn, Comment (1000), CreatedByName, CreatedAt |
| OrderFiles | Generated int id, OrderId → Orders (cascade), ItemId → Items (set null; null = whole order). Kind (Sizes/Application/Extra/Photo), Name (255), ContentType, Bytes, Url (CDN or old Google Drive link), StorageKey (CDN key; null for an old link), CreatedByName, CreatedAt, DeletedAt |
| TelegramDeliveries | Key OrderId → Orders (cascade, one row per order). Status (NotSent/Sending/Sent/Failed), ChatId, ThreadId, MessageId, Attempts, LastError (500), SentByName, SentAt, Version (concurrency) |
| Settings | One row, Id = 1: TelegramEnabled, TelegramChatId (a Reminders bot group as a value), TelegramThreadId, UpdatedByName, UpdatedAt |
| OrderChanges | Generated bigint id, OrderId → Orders (cascade). ItemId, ItemName, Field, Before, After (display texts), ActorName, At |

The model code and user id are plain values; there are no foreign keys to `catalog`, `crm`, `reference` or `identity`.
