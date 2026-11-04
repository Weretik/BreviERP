# Production schema

Code model reviewed 2026-11-04. `Production.Infrastructure.Persistence.ProductionDbContext` owns schema `production`; migration history is `production.__EFMigrationsHistory`. Migration `InitialProduction` creates Employees and FabricNorms ([PRD-001](../../specs/production/001-workshop-references/README.md)); `ProductionOrders` creates Orders, Items and OrderChanges ([PRD-002](../../specs/production/002-production-orders/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| Employees | Generated int id. FullName (120), NameKey (120, lower case), Position (80), IsActive, CreatedAt, UpdatedAt, UpdatedByUserId (Identity user id as a value). Unique NameKey among active rows (filtered index `"IsActive"`) |
| FabricNorms | Generated int id. Unique ModelCode (32, Catalog model code as a value), OneColor, TwoColorsMain, TwoColorsAdditional, ThreeColorsMain, ThreeColorsAdditional1, ThreeColorsAdditional2 (numeric(7,3) metres, nullable), UpdatedAt, UpdatedByUserId |
| Orders | Generated int id. Unique Number, QuoteId (Crm КП id as a value), QuoteNumber (40), CustomerId, ClientName (300), ManagerUserId, ManagerName (200), Status (New/Closed/Cancelled), OrderDate, Comment, PriceTier (Retail/Wholesale/Large), unique CreateRequestId and LegacyId (filtered), CopiedFromOrderId, CreatedAt, CreatedByName, UpdatedAt, DeletedAt (soft delete), Version (concurrency) |
| Items | Generated int id, OrderId → Orders (cascade), unique (OrderId, Position). Snapshot: QuoteItemId, QuoteOptionId, ProductId, ArticleId, ModelCode, ModelName, FabricId, FabricCode, FabricName, SewingRate, ReportUnitPrice (numeric(12,2)). Details: Quantity, DueDate, MainColor, AdditionalColors (jsonb list), SizesText, TechnicalChanges, Comment, Applications (jsonb list of kind/place/description/color), PlannedFabricMeters |
| OrderChanges | Generated bigint id, OrderId → Orders (cascade). ItemId, ItemName, Field, Before, After (display texts), ActorName, At |

The model code and user id are plain values; there are no foreign keys to `catalog`, `crm`, `reference` or `identity`.
