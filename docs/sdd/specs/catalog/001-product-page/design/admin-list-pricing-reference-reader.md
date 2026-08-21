# Product page — вузький Reference reader для цін admin list

**Статус:** реалізовано без cache; performance verification непорожньої page очікує `05.3`
**Scope:** внутрішній read path `GET /api/v1/products`; публічний HTTP/OpenAPI contract,
Product schema та правила розрахунку не змінюються.

## Проблема

Поточний list handler після завантаження page викликає повний
`IProductReferenceReader.GetSnapshotAsync()`. Host adapter послідовно запускає загальні
Reference UI queries для suppliers, fabrics, accessories, operations і additional
references. Частина цих queries виконує додаткові lookup-запити для відображуваних імен.

Admin list не показує supplier або garment-part names і для ціни потребує тільки:

- `Fabric`: `Id`, `Price`;
- `GarmentAccessory`: `Id`, `Price`;
- `GarmentPartOperation`: `Id`, `Min`;
- `AdditionalReference`: `Id`, `Key`, `Value`, `Unit` лише для відсотків поточного PPE
  page і allowlisted ключів Sewing pricing.

Завантаження повних UI DTO та всіх довідникових записів є over-fetching і додає серію
послідовних database round trips до кожного page request.

Для порожнього списку цей шлях не виконується: handler повертається до виклику
`GetSnapshotAsync()`. Перший повільний empty-list request виявився cold start, тому
підфаза не є виправленням already-verified warm empty-list latency.

## Production design

1. Після page projection Catalog збирає distinct IDs тільки з Product rows цієї сторінки:
   fabrics, accessories, operations і AdditionalReferences. Для порожнього набору не
   виконується відповідний запит.
2. Catalog Application вводить вузьку абстракцію
   `IProductListPricingReferenceReader`; вона повертає лише числові pricing inputs,
   потрібні `MinimumWholesalePrice`.
3. Host adapter мапить primitive ID sets у спеціалізований Reference Application query.
   Reference не залежить від Catalog DTO або entities.
4. Reference query виконує set-based projections (`WHERE id IN (...)`) без UI lookups,
   supplier names або garment-part names. Кількість database commands bounded типами
   reference data, а не Product rows у page.
5. Handler будує `ProductReferenceData` або еквівалентний list-only pricing model у пам’яті
   й застосовує наявні формули без зміни значень/округлення.

## Кешування

Основна оптимізація — data narrowing, а не cache. Cache дозволений лише для маленького,
allowlisted набору стабільних Sewing pricing coefficients (`AdditionalReference`), а не для
повного Reference snapshot. Перед додаванням cache реалізація має підтвердити:

- TTL є коротким і явно задокументованим;
- існує наявний механізм invalidation після Reference write, або cache **не додається**;
- кеш не приховує зміну `Fabric.Price`, accessory price, operation minutes або PPE percent;
- поведінка в кількох Host instances визначена (distributed invalidation або TTL-only
  компроміс з погодженим допустимим staleness).

Не додавайте `IMemoryCache` або distributed cache «про всяк випадок». За відсутності
погодженої invalidation strategy вузькі DB projections залишаються єдиною реалізацією.

Пошук поточної codebase не виявив Reference cache invalidation або distributed cache
semantics для Fabric, GarmentAccessory, GarmentPartOperation чи AdditionalReference writes.
Тому реалізація свідомо не додає cache: кожна list page читає актуальні, але вузькі
pricing projections.

## Незмінні властивості та критерії

- List повертає незмінні filtering, sorting, paging, `categoryIds`, `mainPhoto` і
  `minimumWholesalePrice` для Sewing/PPE/fallback `0`.
- Для одного page не існує запитів на кожен Product; SQL command count bounded.
- Full `IProductReferenceReader` залишається для create/update/detail, де повні
  довідникові дані справді потрібні.
- Integration tests порівнюють ціни старого повного snapshot і нового вузького reader
  на тому самому наборі Product/Reference data.
