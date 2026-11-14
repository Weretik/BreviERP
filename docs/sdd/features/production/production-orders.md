# Production orders («Виробничі завдання», stages 2–3)

Managers pass their КП «під запит» to the workshop (saved or sent, since the PDF is often sent by hand; «Передати в цех» in the КП editor opens «Нове завдання» with it, and the editor lists the КП's orders through `quoteId`); the head of the workshop sees every order with the sewing rate and planned fabric; Admin and Director also see the КП price kept for reports. Changes are written to «Історія змін». Old orders of the «Цех» move in through an import; their КП prices, which the old app did not keep, come in through «Ціни КП» (only an item without a price takes one). After «Передати в цех» the head of the workshop ticks stages and records shipments; the order closes when everything is shipped or is cancelled with a reason; files live on the CDN; one Telegram message goes to the workshop group chosen in «Налаштування цеху». The client card tab «Завдання в цех» lists orders by `customerId` (taken from the КП; old imported orders have none). SDD: [stage 2](../../specs/production/002-production-orders/README.md), [stage 3](../../specs/production/003-order-tracking/README.md); [contract](../../contracts/production/production-orders.openapi.yaml), [tables](../../architecture/data-model/production.md). The main and additional colours of an item are picked only from «Кольори» (`GET /api/production/orders/colors`; a colour the item already has stays valid on edit, [REF-006](../../specs/reference/006-fabric-colors/README.md)). The list filters by client, order period, model, fabric, colour and season, and «Зведення за моделями» (`model-summary`) groups the pieces of every filtered order by model with season and category ([PRD-010](../../specs/production/010-order-filters-model-summary/README.md)). The list sorts by number, nearest «Готово до» or order date (`sort`), each row lists its items with stage and the shipped pieces, and the page returns the stage «конвеєр» and the overdue / due-soon counts of «Нове» orders ([PRD-011](../../specs/production/011-order-conveyor/README.md)).

## Code structure

```text
src/Modules/Production/
  Production.Domain/Production.Domain/Orders/
    ProductionOrder.cs           # aggregate: header, items, history lines, soft delete, version
    ProductionItem.cs            # item + ItemSource (snapshot from КП/Catalog) + ItemDetails (what people fill in)
    ProductionOrderChange.cs     # a line of «Історія змін»
    ItemApplication.cs, FabricPlan.cs, OrderText.cs, ProductionOrderEnums.cs
    ProductionStages.cs          # stage labels and order
    ModelCategory.cs             # «Категорія» of «Зведення за моделями», read from the model name
    ProductionShipment.cs, ProductionOrderFile.cs, ProductionTelegramDelivery.cs
  Production.Domain/Production.Domain/Settings/WorkshopSettings.cs   # «Налаштування цеху»: group, topic, on/off
  Production.Application/Production.Application/
    Abstractions/                # IProductionQuoteReader, IProductionFabricReader, IProductionStaffReader, IProductionModelReader,
                                 # IProductionFileStorage, IProductionTelegram, IProductionEmbroideryReader
    Features/Orders/
      Queries/                   # GetProductionOrders (sort, stage conveyor, attention), GetProductionOrderFilterOptions,
                                 # GetProductionModelSummary («Зведення за моделями»), GetProductionOrder, GetProductionOrderHistory,
                                 # GetProductionQuoteSources, GetProductionQuoteDraft, GetQuotesWithOrders, GetProductionEmbroideries (picker, PRD-D41)
      Commands/                  # Create/Update/Copy/DeleteProductionOrder, ImportProductionOrders;
                                 # SetProductionItemStages, Create/CancelProductionShipment, Close/Cancel/ReopenProductionOrder;
                                 # Upload/DeleteProductionOrderFile; SendProductionOrderTelegram («Надіслати в цех»);
                                 # ImportProductionProgress (stages, file links and Telegram marks of old orders),
                                 # ImportProductionReportPrices («Ціни КП» of old orders for «Звіти цеху»)
      Shared/
        Access/ProductionActor.cs  # who may create, edit, delete; which prices they see
        Validation/              # item details and application validators, shared rules
        OrderTracking.cs         # stages, shipments, status changes
        ProductionOrderFiltering.cs, OrderCards.cs (order + files + Telegram mark), QuoteItemResolver.cs
        OrderMapping.cs, OrderNorms.cs, OrderNumbers.cs, OrderActors.cs, OrderMessages.cs, inputs and responses
    Features/Settings/           # GetWorkshopSettings, UpdateWorkshopSettings, SendWorkshopTelegramTest; Shared/WorkshopSettingsReading
  Production.Infrastructure/.../Persistence/Configurations/ProductionOrderConfiguration.cs, ProductionTrackingConfiguration.cs, JsonColumn.cs
  Production.Api/Production.Api/Orders/ProductionOrdersController.cs   # /api/production/orders
  Production.Api/Production.Api/Orders/ProductionOrderTrackingController.cs   # stages, shipments, status, files, telegram, progress-import, report-price-import
  Production.Api/Production.Api/Settings/WorkshopSettingsController.cs        # /api/production/settings
src/Modules/Production/Production.Contracts/Production.Contracts/{Orders,FabricPlans}/  # IQuoteOrderLinks, IOrderFabricPlans for other modules
src/Modules/Production/Production.Infrastructure/Production.Infrastructure/Adapters/
  Crm/ProductionQuoteReader.cs       # Crm.Contracts IQuoteReader, sewn items and «Вишивка» rows
  Crm/ProductionEmbroideryReader.cs  # Crm.Contracts IEmbroideryCalculationReader, without prices
  Catalog/ProductionModelReader.cs   # Catalog.Contracts ISewingModelReader, with the seamstress labour
src/Modules/Production/Production.Infrastructure/Production.Infrastructure/Adapters/
  Reference/ProductionReferenceReaders.cs  # ProductionFabricReader over Reference.Contracts IFabricDirectory
  Identity/ProductionStaffReader.cs        # Identity.Contracts IStaffDirectory, by id and email
  Media/ProductionFileStorage.cs           # Media.Contracts ICdnStorage
  Reminders/ProductionTelegram.cs          # Reminders.Contracts ITelegramBot + the bot's groups
```

## Structure diagram

```mermaid
flowchart LR
  Admin[Brevi Admin «Виробничі завдання»] --> Controller[ProductionOrdersController]
  Controller --> UseCases[Order use cases]
  UseCases --> Db[(production.Orders / Items / OrderChanges)]
  UseCases --> Quotes[IProductionQuoteReader] -.host.-> Crm[Crm quotes]
  UseCases --> Models[IProductionModelReader] -.host.-> Catalog[Catalog sewing card]
  UseCases --> Fabrics[IProductionFabricReader] -.Reference.Contracts.-> Reference[Reference fabrics]
  UseCases --> Staff[IProductionStaffReader] -.Identity.Contracts.-> Identity[Identity users]
  UseCases --> Files[IProductionFileStorage] -.Media.Contracts.-> Media[Media CDN storage]
  UseCases --> Bot[IProductionTelegram] -.Reminders.Contracts.-> Reminders[Reminders bot and groups]
  UseCases --> Embroidery[IProductionEmbroideryReader] -.host.-> CrmEmb[Crm «Ціна вишивки»]
```

## Tests

- `tests/UnitTests/Production/ProductionOrderHandlerTests.cs` — create, idempotency, access, hidden prices, history, copy, delete, list filters, import (SQLite in memory, fake ports).
- `tests/UnitTests/Production/ProductionOrderFilterTests.cs` — filters, sort, stage conveyor and attention, model summary, filter options, category from the name.
- `tests/UnitTests/Production/ProductionTrackingHandlerTests.cs` — stages, shipments, close/cancel/reopen, files, Telegram once, settings, progress import, КП prices of old orders.
- `tests/UnitTests/Production/ProductionOrdersContractTests.cs` — DTOs and operations against the contract.

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production`.

**Data:** [production schema](../../architecture/data-model/production.md)
