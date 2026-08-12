# Product page — data model

## Прийнята стратегія зберігання

Використовується composition-модель: спільна таблиця Product із discriminator `Type` та окремі таблиці деталей для кожного типу. Це не дві незалежні сутності товару і не одна широка таблиця з великою кількістю nullable-полів.

```text
catalog.Products
├── Id, Name, RuName, Slug, Type, DescriptionUk, DescriptionRu, CreatedAt, UpdatedAt, IsDeleted
├── ProductPhotos
└── ProductCategoryLinks

catalog.ProductInformationBlocks
├── Id, ProductId, TitleUk, TitleRu, TextUk, TextRu, SortOrder

catalog.ProductCharacteristicTables
├── Id, ProductId, TitleUk, TitleRu, SortOrder
└── ProductCharacteristicRows
    └── Id, TableId, LabelUk, LabelRu, ValueUk, ValueRu, SortOrder

catalog.SewingProductDetails             лише Type = Sewing
├── ProductId (PK + FK → Products)
└── MetersPerProduct

catalog.SewingProductFabrics
├── ProductId
├── FabricId
├── IsPrimary                              не більше двох `true` на Product
└── SortOrder

catalog.SewingProductAccessories
├── ProductId
├── GarmentAccessoryId
├── Quantity
└── SortOrder

catalog.SewingProductOperations
├── ProductId
└── GarmentPartOperationId

catalog.PpeProductDetails                лише Type = Ppe
├── ProductId (PK + FK → Products)
├── SupplierId
├── BasePrice
├── RetailCoefficientSource              Reference | Custom
├── RetailAdditionalReferenceId?         лише Reference
├── RetailCustomPercent?                 лише Custom
├── WholesaleCoefficientSource           Reference | Custom
├── WholesaleAdditionalReferenceId?      лише Reference
└── WholesaleCustomPercent?              лише Custom
```

Ціни Sewing є обчислюваним read-model для кожної пари «Product — Fabric» і не зберігаються окремими таблицями: формула використовує актуальні ціни Reference та `AdditionalReference`. Історія цін і виробничих норм не зберігається. Поточні `Products`, `ProductPhotos` і `ProductCategoryLinks` уже існують; усі інші структури в схемі вище — цільові та ще не реалізовані.

### Правила цілісності

- `Products.Type` визначає, яка detail-модель дозволена.
- Sewing має рівно один `SewingProductDetails` і не має `PpeProductDetails`.
- Ppe має рівно один `PpeProductDetails` і не має жодної Sewing detail/collection.
- `SewingProductDetails.ProductId` та `PpeProductDetails.ProductId` є одночасно PK і FK: не може існувати більш ніж один запис деталей на товар.
- Кожен зв’язок, що належить Product, має явний CLR-FK (`ProductId`, а для рядків характеристик — `TableId`). EF Core shadow properties для цієї моделі не використовуються. Link-таблиці Sewing мають складений первинний ключ з `ProductId` та ID довідника.
- `Products.Slug` унікальний у межах Catalog.
- `Products.Name` та `Products.RuName` кожне унікальне у межах Catalog.
- Product успадковує audit-поля `CreatedAt`, nullable `UpdatedAt` та `IsDeleted` із `BaseAuditableEntity<ProductId>`. Час надходить до Domain із Application/clock: у Domain немає викликів `DateTimeOffset.UtcNow`. `CreatedAt` встановлюється сервером під час створення, `UpdatedAt` — під час кожної успішної зміни. Дати використовуються для admin-сортування; поточний delete Product залишається фізичним, тому `IsDeleted` у цьому delivery не встановлюється.
- Для одного Product не більше двох `SewingProductFabrics.IsPrimary = true`; порядок read-model: `IsPrimary` за спаданням, потім `SortOrder` за зростанням.
- `ProductInformationBlocks` і `ProductCharacteristicTables` належать одному Product; `ProductCharacteristicRows` належить одній таблиці. Порядок у кожній колекції визначає `SortOrder`.
- Перевірки `Type` і взаємовиключних полів залишаються у Domain/Application; БД забезпечує ключі, FK та унікальність зв’язків.
