# EMB-001 — «Ціна вишивки» (embroidery price calculator)

**Status:** stage 1 (Розрахунок + Налаштування) implemented 2026-11-04. **Date:** 2026-11-04. **Module:** Crm («Менеджер сервісу»), schema `crm`. Feature card: [embroidery price](../../../features/crm/embroidery-price.md). Contract: [embroidery.openapi.yaml](../../../contracts/crm/embroidery.openapi.yaml).

## Why

The owner asked on 2026-11-04 (project thread «Port Brevi embroidery price calculator») to bring the «Ціна вишивки» section of the old calculator (calculator.brevi.com.ua, version 217, technical specification «Перенос раздела Ціна вишивки» v1.0) into «Менеджер сервісу», after a UI/UX audit. The owner chose concept A «Форма + живий підсумок» and these rules (2026-11-04 20:05):

- Managers see the calculation and the prices; the cost and «Налаштування» only Director and Admin.
- The three markups are named «Опт», «Стандарт», «Терміново».
- The embroiderer's rate is one number: the pricing parameter `EMBROIDERY_RATE_1000_STITCHES` of «Параметри ціноутворення», the same the workshop payroll uses.

## Decision: inside Crm

The calculator is a tool of «Менеджер сервісу» and later feeds КП («Додати в КП», stage 3), so it lives in Crm next to КП. It reads the stitch rate from Reference through `IEmbroideryStitchRateReader` (adapter in Host.Api), never a copy.

## Rules

- **R-01:** The formula is the old calculator's, exact: hoopings `H = ceil(N/K)` (K = chevrons per hooping, 1 for a garment); stitches `T = N·S/1000`; stabilizer and fabric blanks `H × layers`, rows rounded once for all layers, no rotation; fabric blank = stabilizer blank × (1 + increase) on both sides; used-blanks charge `Q × P·(B/100)/across`, full-cut charge `P × rows·B/100`; program `minutes/60 × rate` once per batch, zero when ready; each extra cost × its basis (1000 stitches, hooping, unit, machine hour, order).
- **R-02:** Prices are computed on the server. A Manager gets prices, hoopings and cut layout, never the cost, rates, markup percents or material money.
- **R-03:** Settings: thread, hooping and program rates, tax percent, exactly three named markups, two materials (price per metre, roll width), fabric increase, 1–30 hoops (id, label, blank size, optional own glue rate), up to 100 extra costs (name, basis, rate or empty, enabled by default, all or chevrons only).
- **R-04:** An empty rate is «not set»: the item is skipped and named in `missing`, the price is «Орієнтовна». Zero is free; an unticked item is nothing. A machine-hour cost with a positive rate and no machine time adds «Час роботи машини» to `missing`.
- **R-05:** Glue takes the chosen hoop's own rate when set, zero included.
- **R-06:** Price per unit = round(C/N × (1 + markup) × (1 + tax)); order = round(unit × N); repeat = the same without the program cost.
- **R-07:** Exact fractions until the end; money rounded half-up to kopecks only in the result.
- **R-08:** Settings save whole, Director/Admin only, 409 on a stale version. Before the first save the old calculator's rates of 2026-11-04 apply (version 0).
- **R-09:** A missing stitch rate in «Параметри ціноутворення» stops the price with a message that says where to set it.
- **R-10 (stage 2):** «Зберегти розрахунок» sends the form, an optional name and an optional client; the server prices it with the rates in force now and stores the form, the stitch rate and the whole settings content, and the result. A saved record is never recalculated or edited; «Повторити розрахунок» loads its form into «Розрахунок» with the current rates. An «Орієнтовна» price can be saved; a failed one (`error`) or an unknown or hidden client is 400. The name is trimmed and cut to 120; empty is «Без назви».
- **R-12 (stage 2):** «Додати в КП» appends a saved calculation to a КП «Під запит» the actor may edit (`POST /api/crm/quotes/{id}/embroidery`) as one `Embroidery` item: name = the saved name (or «Вишивка»/«Шеврони»), subtitle «Вишивка · ВШ-0042», no description (round 2 follow-up, 2026-11-09: the client does not need quantity, stitches or hoop in the КП; the manager may write one), one row titled with the chosen level, details «Повтор з готовою програмою — … грн за 1 шт.». The level's saved unit price stands, shown, in all three КП tiers (owner's choice, 2026-11-04). It is never repriced; the editor may change only the description and the «показати» ticks, and cannot create one. History records «EmbroideryAdded».
- **R-13 (stage 2):** «Імпорт архіву» (`POST /api/crm/embroidery/calculations/import`, Director and Admin) moves the old calculator's saved calculations from a transfer file built from its D1 export. Each record keeps its old date and author (a staff member matched by email, otherwise the email), freezes its own old stitch rate and «Налаштування», is priced with them and must give the old saved prices to the kopeck. A record already moved (same old id) is left as it is; a list with any error saves nothing; `dryRun` only shows the lines.
- **R-14 (stage 4):** «Макет»: a saved calculation may have one photo or mockup of the design picked from the Media library (`PUT /api/crm/embroidery/calculations/{id}/mockup`, everyone who calculates; `null` clears it). It is the only thing that changes after saving. An unknown photo is 400. The photo cannot be deleted from the library while a calculation uses it (area «Вишивка»); a mockup whose photo is gone shows as none.
- **R-15 (stage 4):** The workshop link: the «Вишивка» of a production order item may point to a saved calculation (PRD-D41). The order card shows its number, name, quantity, stitches, hoop, hoopings, the metres of флізелін and fabric to cut and the mockup, as they are now, without any price. Materials are only shown; the fabric stock is not changed (owner's choice, 2026-11-05).
- **R-16 (client corrections round 2, 2026-11-08):** Two price categories instead of three: «Опт» is removed, «Стандарт» becomes «Категорія 1» and «Терміново» «Категорія 2» (the saved settings keep their percents; migration `EmbroideryPriceCategories`). «Зберегти розрахунок» also sends `category`, one of the result's price names, and the record keeps only that price: the list, the card and «Додати в КП» show one price, and the КП row is titled with the category. A calculation saved before (including the archive) stands for «Стандарт», shown as «Категорія 1». The copy icon of a price copies its unit price only. Access rights did not change.
- **R-11 (stage 2):** «Збережені» lists all records newest first, 25 per page, search by name, client or number «ВШ-0042». Everyone who calculates sees every record; a Manager gets no cost and no frozen rates. Only Director/Admin delete.

Deliberate changes from the old calculator: Managers can calculate (the old one was director-only); the stitch rate comes from «Параметри ціноутворення»; markups have names; calculation inputs have upper limits against typing mistakes (1 000 000 units or stitches, 20 layers, 500 per hooping); codes in the API are names (`Garment`, `UsedBlanks`).

## Scenarios

| ID | Given | When | Then |
| --- | --- | --- | --- |
| SC-01 | Spec example A (N=50, S=10 000, 18 см, new program 60 хв, extras 0) | Calculate | Cost 1505; 59.72 / 70.92 / 82.11; repeat 47.81 / 56.78 / 65.74 |
| SC-02 | Spec example B (chevrons, 32 см, 4 per hooping) | Calculate; ready program; full cut | 13 hoopings, last 2 of 4, cost 1423.20; 1123.20 with 44.57 / 52.93 / 61.28; 1449.60 |
| SC-03 | Hoop 15, 125 chevrons | Calculate | Fabric 504, not 503.75 |
| SC-04 | Empty, zero and unticked items | Calculate | Missing listed; zero row; nothing |
| SC-05 | Manager | Calculate; open settings | Prices without cost; 403 |
| SC-06 | Two windows | Director saves in both | Second save 409 |
| SC-07 | Hooping rate 12 | Manager saves; Director changes the rate to 30; opens the record | Record keeps rate 12, hooping 600 and the same prices; Manager got no cost or rates |
| SC-08 | Two records | Search «поло», «вш-0002» | Newest first; each search finds one |
| SC-09 | Manager | Delete a record | 403; Director deletes, then 404 |
| SC-10 | Saved calculation, own КП | «Додати в КП», level «Стандарт»; save with a new description; copy the КП | One Embroidery row, same price in all tiers; description kept, price unchanged; the copy keeps the link |
| SC-11 | Someone else's КП, unknown level, typed embroidery item | Add; save | 404; 400; 400 |
| SC-12 | Old archive record, the same file again, one record with different prices | Check; import; import again | nothing saved, «ВШ-…» with the old date and author, `unchanged`; the whole list is refused |
| SC-13 | Saved calculation, a library photo, an unknown photo | Set the mockup; set the unknown one; delete the photo in «Медіа»; clear | the list and the card show it; 400; the library refuses (used by «Вишивка»); none |

Evidence: `tests/UnitTests/Crm/Embroidery/` (SC-01…SC-05), `tests/IntegrationTests/Crm/EmbroideryPersistenceTests.cs` (SC-05, SC-06), `tests/UnitTests/Crm/Embroidery/EmbroiderySavedTests.cs` and `tests/IntegrationTests/Crm/EmbroiderySavedPersistenceTests.cs` (SC-07…SC-09), `tests/IntegrationTests/Crm/QuoteEmbroideryPersistenceTests.cs` and `tests/UnitTests/Crm/Quotes/QuoteEmbroideryTests.cs` (SC-10, SC-11), `tests/IntegrationTests/Crm/EmbroideryArchivePersistenceTests.cs` (SC-12), `EmbroiderySavedPersistenceTests.MockupIsPickedFromTheMediaLibraryAndBlocksItsDeletion` (SC-13); the workshop link is in `tests/UnitTests/Production/ProductionOrderHandlerTests.cs`. The old calculator's 28 golden vectors were not part of the handed-over files; add them when the owner provides `fixtures/golden-vectors.json`.

## Data

Migration `AddEmbroideryMockup` (stage 4): `crm.EmbroideryCalculations.MockupMediaFileId` (nullable int, no FK to `media`). Rollback: the previous image ignores the column.

Migration `AddEmbroiderySettings`: table `crm.EmbroiderySettings` — `Id` (always 1), `Content` (jsonb, the whole page), `UpdatedByName`, `UpdatedAt`, `Version` (concurrency). Rollback: the previous image ignores the table.

Migration `AddQuoteEmbroideryItems` (stage 2): `crm.QuoteItems.EmbroideryCalculationId` (nullable, indexed, no FK: deleting a saved calculation leaves the КП as it is), check `CK_QuoteItems_Embroidery`, and `CK_QuoteItems_Product` allows `Embroidery` without a product. Rollback: the previous image cannot read КП that hold an Embroidery item; delete those items first.

Migration `AddEmbroideryCalculations` (stage 2): table `crm.EmbroideryCalculations` — generated `Id` (shown as «ВШ-0042»), `Name`, optional client `CustomerPublicId` with `CustomerName` (no FK), `Mode`, `Quantity`, jsonb `Input`, `Snapshot` (stitch rate + settings content) and `Result`, `Incomplete`, creator id and name, `CreatedAt` (indexed). Rollback: the previous image ignores the table.

## Stages

1. Розрахунок + Налаштування (this change). Rates transfer: the Director checks the defaults on «Налаштування» and saves.
2. «Збережені»: saved calculations with a frozen rate snapshot, client from Crm, search (done). Transfer of the old archive (R-13): «Імпорт архіву» for Director and Admin, check first, then import; each old record keeps its date, author and rates, is priced with those old rates and must give the old saved prices to the kopeck. The export of 2026-11-05 held 9 records; all 9 gave the old prices exactly, and the one test record is left out of the transfer file.
3. «Додати в КП» — done in stage 2 (R-12). «+ Вишивка» inside a КП (adding a saved calculation from the editor) is not planned yet.
4. Links to production orders, photos and materials (R-14, R-15): «Макет» from «Медіа», the «Вишивка» of an order item points to «ВШ-…» and is filled in from the КП, materials are shown in the order card and not taken from stock.
