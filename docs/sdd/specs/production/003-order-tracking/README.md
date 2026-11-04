# PRD-003 «Швейний цех»: stages, shipments, files and Telegram

Status: stage 3 implemented 2026-11-04. Follows [PRD-002](../002-production-orders/README.md).

## Goal

After «Передати в цех» the head of the workshop ticks how far each item is, records what went to the client, and the order closes when everything is shipped. Files for sewing live with the order on the CDN. The workshop group gets one message about a new order. The progress of the 28 orders moved from the old «Цех» comes with them.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D17 | Stages of an item: Нове, Тканина замовлена, Тканина в наявності, Крій, Нанесення, Пошив, Готово до відвантаження, Фактично відвантажено, Завершено. Several can be ticked; they are kept in this order without repeats; none ticked means «Нове». The furthest one is `CurrentStage` and colours the row. The order list shows and filters by the least advanced item. |
| PRD-D18 | Stages and shipments: WorkshopChief and Admin, only on a «Нове» order (`CanTrack`), as in the old app. Every change is a line of «Історія змін». |
| PRD-D19 | A shipment is quantity, date and comment for one item; the total never exceeds the item quantity (409), and the quantity cannot be edited below what is shipped (400). Shipping the rest ticks «Фактично відвантажено». A wrong shipment can be cancelled. |
| PRD-D20 | Status: «Закрити» needs every item shipped in full (WorkshopChief, Admin); «Скасувати» needs a reason (whoever may edit the order); «Повернути в роботу» is Admin only and clears the reason. |
| PRD-D21 | Files (sizes, application layout, extra, photo) of the order or of one item go straight to the Media CDN storage under `/production/orders/<number>/`, any type up to 20 MB; deleting hides the row and removes the CDN file. Files of the old «Цех» stay as their Google Drive links (`IsOldLink`); its КП photos are not moved. |
| PRD-D22 | Telegram uses the existing Brevi bot of «Нагадування». «Налаштування цеху» (WorkshopChief, Admin) picks one of the bot's groups and an optional topic; sending is off until turned on; a test line checks it. |
| PRD-D23 | «Надіслати в цех» sends one message per order (number, client, manager, items, file links, link to the order). The row is marked «Надсилається» and saved before sending, with a concurrency token, so two presses never send twice; a sent message is never sent again; a failure keeps its reason on the card and can be retried. Orders moved from the old app whose message went out there are marked sent. |
| PRD-D24 | Progress of old orders moves through `POST /api/production/orders/progress-import` (Admin), matched by old id and item position, with a preview first; nothing changes unless every row is fine; a repeat changes nothing. |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-003-A1 | Stages are kept in order without repeats, the furthest is current; a manager cannot tick them. |
| PRD-003-A2 | Shipments never exceed the quantity; a full one ticks «Фактично відвантажено»; a cancelled one returns its quantity. |
| PRD-003-A3 | Close needs everything shipped; cancel keeps the reason; only Admin reopens. |
| PRD-003-A4 | A file goes to the order's CDN folder with its name; deleting removes it from the card and the CDN. |
| PRD-003-A5 | The Telegram message is sent once, to the chosen topic, with the order link; a failure is shown and can be retried. |
| PRD-003-A6 | Settings are for WorkshopChief and Admin and list the bot's groups. |
| PRD-003-A7 | Progress import: an error saves nothing, a preview saves nothing, a repeat is unchanged. |

## File-role map

| Role | Files |
| --- | --- |
| Domain | `Orders/ProductionStages.cs`, `ProductionShipment.cs`, `ProductionOrderFile.cs`, `ProductionTelegramDelivery.cs`; `ProductionOrder` (Ship, CancelShipment, Close, Cancel, Reopen, SetItemStages), `ProductionItem.SetStages`; `Settings/WorkshopSettings.cs` |
| Use cases | `Orders/Tracking/` (OrderTracking, stages, shipments, status), `Orders/Files/`, `Orders/Telegram/` (message, send), `Orders/ImportProgress/`, `Settings/WorkshopSettingsCommands.cs`; `Orders/OrderCards.cs` builds the card |
| Ports | `Abstractions/IProductionFileStorage.cs` (Media storage), `IProductionTelegram.cs` (Reminders bot and groups) |
| Persistence | `Configurations/ProductionTrackingConfiguration.cs`, Shipments in `ProductionOrderConfiguration.cs`; migration `ProductionTracking` |
| API | `Production.Api/Orders/ProductionOrderTrackingController.cs`, `Settings/WorkshopSettingsController.cs` |
| Host | `Host.Api/Production/ProductionFileStorage.cs`, `ProductionTelegram.cs`; `ITelegramGateway.SendTopicMessageAsync` in Reminders; `Production:AdminUrl` for the link |
| Contract | `docs/sdd/contracts/production/production-orders.openapi.yaml` |

## Evidence

- Unit: `dotnet test tests/UnitTests/UnitTests.csproj` — 629 passed (2026-11-04), including `ProductionTrackingHandlerTests` (9) and `ProductionOrdersContractTests`.
- Architecture: `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj` — 13 passed.
