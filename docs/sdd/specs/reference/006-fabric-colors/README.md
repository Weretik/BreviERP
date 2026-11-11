# REF-006 — Colours («Кольори») and colours picked from the list in the workshop

**Status:** backend implemented and verified 2026-11-11; Admin tab in brevi-web. **Date:** 2026-11-11. **Module:** Reference, schema `reference`; Production reads it through the host.

The owner asked on 2026-11-11 for a colours reference «Кольори» as a separate tab of «Матеріали» (next to «Тканини» and «Фурнітура»), filled with the colours of the client sheet, because colours take part in the workshop calculations: a stock line and its reservations are keyed by fabric and colour, and an item plans fabric by its main and additional colours.

## Source

Client sheet `brevi.xlsx` of 2026-11-11, tab «Тканини», column «Загальний перелік кольорів»: 61 colours, from «Чорний» to «Вафля». The owner sent the same list in the thread.

## Decisions

- **D-01 (owner, decision card 2026-11-11, «Только из справочника»):** the workshop picks a colour only from the active colours of «Кольори»: an item's main and additional colours and the colour of a new stock line («Початковий залишок», «Прихід»). Free text is rejected with 400.
- **D-02:** a colour already saved on the record stays valid: editing an item may keep a colour that is no longer in the list, and a receipt to an existing stock line keeps the line's colour. Imports from the old «Цех» are not checked.
- **D-03:** records keep the colour as text (as before), so a rename or delete in «Кольори» does not change saved items or stock lines. The match uses the stock key: case and outer or repeated spaces are ignored.
- **D-04:** a name that differs from another colour only in case or spaces is a duplicate.
- **D-05:** reads are `CatalogRead`; writes are `access:materials` (the «Матеріали» page of «Права доступу»; no new page key). The workshop reads the colours through Production endpoints, so it needs no access to «Довідники».
- **D-06:** the 61 colours arrive with the migration, in sheet order (`SortOrder` 1–61), so no manual import is needed.

## Scenarios

- **SC-01 / D-04:** creating «світло - сірий» next to «Світло-сірий» returns 400; a new colour takes the next ID.
- **SC-02 / D-01:** an order with the additional colour «Бузковий» (not in the list) is rejected and nothing is saved.
- **SC-03 / D-02:** after «Червоний» leaves the list, editing an item that has it succeeds; replacing the main colour by «Бузковий» returns 400.
- **SC-04 / D-01:** an opening of a new stock line with «Бузковий» returns 400; with «Жовтий» it is saved; the stock form choices list the colours.

## Design

- Domain: `Reference.Domain/FabricColors/` (`FabricColor`: `Name` ≤ 80, the same limit as a stock colour, `IsActive`, `SortOrder`).
- Application: `Features/FabricColor/` (Create, Update, Delete, GetList, `Shared/FabricColorNames` duplicate key).
- API: `api/reference/fabric-colors` GET/POST/PUT/DELETE. Contract: [fabric-colors.openapi.yaml](../../../contracts/reference/fabric-colors.openapi.yaml).
- Production: `IProductionColorReader` (host adapter `ProductionColorReader`, active colours only), `Colors/ProductionColors` (check used by create and update order and by «Початковий залишок» / «Прихід» of a new line), `GET /api/production/orders/colors` for the item form, and `colors` in `GET /api/production/fabric-stock/choices`. Contracts: [production orders](../../../contracts/production/production-orders.openapi.yaml), [fabric stock](../../../contracts/warehouse/fabric-stock.openapi.yaml).
- Data: migration `AddFabricColors` creates `reference.FabricColors` (unique `Name`) and inserts the 61 colours. Additive; rollback is the migration `Down`.

## Tasks

- **TS-01 (done):** domain, migration with the colours, CRUD, list. Tests: `tests/UnitTests/Reference/FabricColorTests.cs`.
- **TS-02 (done):** workshop check and colour lists. Tests: `tests/UnitTests/Production/ProductionColorTests.cs`.
- **TS-03:** Admin tab «Кольори» and colour pickers in brevi-web.

## Verification 2026-11-11

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~FabricColorTests|FullyQualifiedName~UnitTests.Production|FullyQualifiedName~Contract"`: 469 passed.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj`: 19 passed.
- `dotnet ef migrations add AddFabricColors` (Host.Api startup) generated the table; the snapshot matches the model.
