# Product page — data model

## Прийнята стратегія зберігання

Використовується composition-модель: спільна таблиця Product із discriminator `Type` та окремі таблиці деталей для кожного типу. Це не дві незалежні сутності товару і не одна широка таблиця з великою кількістю nullable-полів.

```text
catalog.Products
├── Id, Name, RuName, Slug, Type
├── спільний описовий блок
├── ProductPhotos
└── ProductCategoryLinks

catalog.SewingProductDetails             лише Type = Sewing
├── ProductId (PK + FK → Products)
└── MetersPerProduct

catalog.SewingProductFabrics
├── ProductId
├── FabricId
├── IsPrimary
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
├── CoefficientSource                    Reference | Custom
├── AdditionalReferenceId?               лише Reference
└── CustomCoefficient?                   лише Custom
```

Ціни будуть додані окремими таблицями після погодження формул. Поточні `Products`, `ProductPhotos` і `ProductCategoryLinks` уже існують; всі таблиці Sewing/Ppe у схемі вище — цільові та ще не реалізовані.

### Правила цілісності

- `Products.Type` визначає, яка detail-модель дозволена.
- Sewing має рівно один `SewingProductDetails` і не має `PpeProductDetails`.
- Ppe має рівно один `PpeProductDetails` і не має жодної Sewing detail/collection.
- `SewingProductDetails.ProductId` та `PpeProductDetails.ProductId` є одночасно PK і FK: не може існувати більш ніж один запис деталей на товар.
- Перевірки `Type` і взаємовиключних полів залишаються у Domain/Application; БД забезпечує ключі, FK та унікальність зв’язків.

