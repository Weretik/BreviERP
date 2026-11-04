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

Evidence: `tests/UnitTests/Crm/Embroidery/` (SC-01…SC-05), `tests/IntegrationTests/Crm/EmbroideryPersistenceTests.cs` (SC-05, SC-06). The old calculator's 28 golden vectors were not part of the handed-over files; add them when the owner provides `fixtures/golden-vectors.json`.

## Data

Migration `AddEmbroiderySettings`: table `crm.EmbroiderySettings` — `Id` (always 1), `Content` (jsonb, the whole page), `UpdatedByName`, `UpdatedAt`, `Version` (concurrency). Rollback: the previous image ignores the table.

## Stages

1. Розрахунок + Налаштування (this change). Rates transfer: the Director checks the defaults on «Налаштування» and saves.
2. «Збережені»: saved calculations with a frozen rate snapshot, client from Crm, search; transfer of the old archive without recalculation.
3. «Додати в КП» and «+ Вишивка» inside a КП.
4. Links to production orders, photos and materials.
