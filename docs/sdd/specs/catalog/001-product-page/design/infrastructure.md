# Product page — design: Infrastructure

**Статус:** реалізовано у фазі 02
**Залежить від:** [Domain design](domain.md)  
**Блокує:** Application-сценарії створення та оновлення товару

## Результат

Спільні дані Product зберігаються в `catalog.Products`, а типові дані — у таблицях деталей за composition-моделлю без nullable-полів іншого типу товару.

## Цільові таблиці

| Таблиця | Власник | Призначення |
| --- | --- | --- |
| `Products` | Catalog | спільні властивості та discriminator `Type` |
| `ProductPhotos` | Catalog | наявна owned collection фото |
| `ProductCategoryLinks` | Catalog | наявна owned collection категорій |
| `SewingProductDetails` | Catalog | `MetersPerProduct`, один запис на Sewing товар |
| `SewingProductFabrics` | Catalog | тканини, основна ознака, порядок |
| `SewingProductAccessories` | Catalog | фурнітура, кількість, порядок |
| `SewingProductOperations` | Catalog | посилання на операції трудомісткості |
| `PpeProductDetails` | Catalog | постачальник, ручна базова ціна та окремі режими/значення роздрібного й оптового відсотків |

## EF Core правила

- `SewingProductDetails` і `PpeProductDetails` мапляться як one-to-one detail record із `ProductId` одночасно PK/FK.
- Усі FK у Product-моделі є властивостями CLR-сутностей; рядкові `HasForeignKey("ProductId")`, shadow `ProductId` та shadow surrogate `Id` не допускаються. Для link-таблиць Sewing застосовується складений PK, а не штучний EF-ключ.
- `RetailPricePercent` і `WholesalePricePercent` мапляться як EF Core complex types у `PpeProductDetails`; вони не є окремими таблицями та не дублюють одне одного. Єдиний стан кожного відсотка: `Source`, `AdditionalReferenceId` або `CustomPercent`.
- Sewing collections мають composite unique keys: `(ProductId, FabricId)`, `(ProductId, GarmentAccessoryId)`, `(ProductId, GarmentPartOperationId)`.
- `Products.Slug` має унікальний індекс у межах schema `catalog`.
- Reference IDs зберігаються як типізовані value objects/скаляри. Між module DbContext-ами не додаються EF navigation properties до Reference entities.
- Посилання на Reference не отримують фізичний FK cascade. Перед видаленням `GarmentPartOperation`, `Fabric` або `GarmentAccessory` Reference перевіряє їх використання в Catalog і відхиляє видалення, якщо є хоча б одне посилання.
- Detail table не може замінити Domain/Application перевірку відповідності `Product.Type`.

## Read projections

Admin list використовує `GetAdminProductsSpec : Specification<Product, ProductListItem>` з
`AsNoTracking`, SQL-side filters/sorting/pagination і `Select` у `ProductListItem`.
Repository materialize-ить лише поля рядка списку та потрібні значення owned collections
(`CategoryIds`, ID головного фото), а не повний `Product` aggregate. `COUNT` виконується
тим самим параметризованим specification до pagination. List use case повертає
`Ardalis.Result.PagedResult<IReadOnlyList<ProductListItem>>` з metadata у `PagedInfo`.
Для неунікальних полів сортування (`Name`, `CreatedAt`, `UpdatedAt`) specification додає
`Id` як вторинний ключ у тому самому напрямку, щоб сторінки `Skip`/`Take` мали сталий порядок.

## Міграція

Потрібна нова погоджена EF migration після реалізації Domain-моделі та EF configurations. Не редагувати застосовані міграції. Rollback має видаляти лише нові таблиці до моменту появи production-даних.

## Критерії приймання

- [x] У схемі немає Sewing-specific колонок у `PpeProductDetails` та Ppe-specific колонок у `SewingProductDetails`.
- [x] Один Product не може мати два detail records одного типу.
- [x] Унікальність link-таблиць підтримується БД.
- [x] Міграція застосовується на порожній БД і не змінює наявні таблиці не за scope.
- [x] Domain/Application не залежить від Infrastructure або Reference EF entities.

## Перевірка

- Integration-тест `CatalogProductPersistenceTests` перевіряє EF mapping, PK/FK, unique constraints і lifecycle aggregate-а.
- Тест створює порожню тимчасову PostgreSQL БД, застосовує міграції та перевіряє цільові таблиці.
- Тест відкочує Catalog до `20260727004045_InitialCatalog`, перевіряє зникнення нових таблиць і видаляє тимчасову БД.
- `GetAdminProductsSpecTests` підтверджує SQL filters, sorting, стабільний secondary sort за `Id`, pagination і projection у тимчасовій PostgreSQL БД.
