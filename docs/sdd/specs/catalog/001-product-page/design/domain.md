# Product page — design: Domain

**Статус:** у проєктуванні  
**Залежить від:** прийнятого рішення щодо правил Sewing  
**Блокує:** persistence, Application та API для операцій

## Результат

`Product` може представити спільні дані двох типів товару й набір операцій тільки для `ProductType.Sewing`, не порушуючи межі Catalog та Reference.

## Наявна основа

`Product` уже є aggregate root. Він володіє назвами, slug, type, `ProductPhoto` і `ProductCategoryReference`. `ProductPhoto` посилається на `MediaFileId` і перевіряється aggregate-ом на готовність медіафайлу.

`MediaFile`, який використовується хоча б в одному `ProductPhoto`, не видаляється з Media. Media перевіряє використання в Catalog до видалення й відхиляє операцію, якщо посилання існує; Product не змінюється автоматично.

Поточна технічна база та правила інтеграції з Reference описані в [implementation-baseline.md](implementation-baseline.md).

## Запропоноване проєктування

```text
Catalog.Product (aggregate root)
├── спільні властивості: Name, RuName, Slug, Type, Photos, Categories
├── Description: Markdown українською та російською
├── InformationBlocks[]: локалізовані заголовок, текст і порядок
├── CharacteristicTables[]: локалізовані таблиці та їх рядки
└── SewingOperations[]                                             (лише для Type = Sewing)
    └── GarmentPartOperationId ──→ Reference.GarmentPartOperation
                                      └── Min
```

`SewingOperations` є дочірньою колекцією Product і містить тільки типізований ID довідникової операції. Product не містить `GarmentPartOperation` entity та не залежить від Reference Infrastructure. Application перевірить існування ID через абстракцію читання Reference.

## Прийнятий розрахунок

```text
totalOperationMinutes = Σ operation.Min
piecesPerShift = 480 / (totalOperationMinutes × 1.25)
```

`480` — встановлена тривалість зміни у хвилинах, а `1.25` — фіксований коефіцієнт до сумарної тривалості операцій. `piecesPerShift` є єдиним показником кількості виробів за зміну: він передається повним дробовим числовим значенням без округлення. Окремі значення для завершених або теоретичних виробів і залишку хвилин не обчислюються.

Sewing-товар може існувати як чернетка без операцій; для нього показники не обчислюються. Розрахунок також не виконується, якщо сума `Min` дорівнює нулю. `Min` завжди читається з актуального `GarmentPartOperation` у Reference; Product не зберігає snapshot, тому зміна довідника змінює наступний розрахунок.

## Обмеження видалення довідникових даних

`GarmentPartOperation`, `Fabric` і `GarmentAccessory` не видаляються з Reference, якщо хоча б один Product містить відповідне посилання. Reference перевіряє наявність посилань у Catalog до видалення; якщо посилання є, видалення відхиляється, а Product не змінюється.

Автоматичне очищення links, міжмодульні події, retry та eventual consistency для цього не використовуються.

## Критерії приймання для реалізації

- [ ] `Ppe` не може мати Sewing-операції.
- [ ] Одна операція не додається двічі до одного Product.
- [ ] Некоректний або відсутній operation ID не потрапляє до aggregate-а.
- [ ] Sewing-чернетка без операцій дозволена; трудомісткість для неї не обчислюється.
- [ ] Трудомісткість не ділить на нуль; `piecesPerShift` передається повним дробовим значенням без округлення.
- [ ] Зміна `Min` у Reference змінює наступний розрахунок без зміни Product.
- [ ] Видалення operation, fabric або accessory, що використовується Product, відхиляється без зміни Product.
- [ ] Catalog не отримує залежність від Reference Infrastructure або entity.

## Тканини: доповнення до доменної моделі

`Fabrics` буде дочірньою колекцією Product із полями `FabricId`, `IsPrimary` та `SortOrder`. Вона не містить entity `Fabric`, її `Price` або `ProviderId`. Product забезпечує унікальність тканини й не більше двох основних тканин; якщо в списку лише одна тканина, вона є основною. Read-model відображає основні тканини першими, а потім застосовує `SortOrder`. Application перевіряє існування тканин через Reference abstraction.

`MetersPerProduct` є одним ручним додатним полем самого Sewing-товару: кількістю метрів тканини на один виріб. Воно не належить `Fabrics[]`, не дублюється для кожної тканини та не застосовується до `Ppe`.

Витрату визначено як product-level `MetersPerProduct`. Правила розрахунку трьох цінових діапазонів 1–10 / 11–39 / 40+ шт. для кожної вибраної тканини визначені в [requirements/pricing.md](../requirements/pricing.md). Результат є обчислюваним read-model, а не snapshot у Product: він використовує актуальні ціни Reference і `AdditionalReference`. Fabric, прив’язану до Product, не можна видалити.

Для Ppe потрібне обов’язкове product-level поле `BasePrice`, яке вводить адміністратор. До нього окремо застосовуються роздрібний і оптовий відсотки; для кожного рівня адміністратор обирає `AdditionalReference` або вводить ручний відсоток. Формули та діапазони визначені в [requirements/pricing.md](../requirements/pricing.md).

## Фурнітура: доповнення до доменної моделі

`Accessories` буде дочірньою колекцією Product із полями `GarmentAccessoryId`, `Quantity` та `SortOrder`. Вона не містить entity `GarmentAccessory`, його `Price` або `SupplierId`. Product забезпечує унікальність фурнітури та додатну кількість; Application перевіряє існування ID через Reference abstraction.

Розрахунок вартості використовує актуальну ціну Reference і кількість на виріб відповідно до [requirements/pricing.md](../requirements/pricing.md); дробові результати не округлюються. Фурнітуру, прив’язану до Product, не можна видалити.

## Межа типу товару

`Fabrics`, `Accessories`, `MetersPerProduct` і `SewingOperations` доступні лише для `ProductType.Sewing`. Product із типом `Ppe` не може створювати, зберігати або змінювати ці дані.

`ProductType.Ppe` має рівно один обов’язковий `SupplierId`, що посилається на `Reference.Supplier`, та обов’язковий додатний `BasePrice`. Product не містить entity Supplier; Application перевіряє існування ID через Reference abstraction. Sewing-товар не може мати `SupplierId` або `BasePrice` на рівні Product.

Ppe має два value object-и `RetailPricePercent` і `WholesalePricePercent`, кожен із двома взаємовиключними станами: `Reference(AdditionalReferenceId)` або `Custom(decimal value)`. У першому випадку Application читає актуальне значення довідника Reference з unit `%`; у другому значення належить Product і є невід’ємним. Product не створює AdditionalReference автоматично. Sewing-товар не може мати цих відсотків.

## Перевірка

- Unit-тести: add/remove/replace operations, duplicate ID, type boundary, zero total, calculation without rounding.
- Integration-тести: перевірка наявності Reference operation, відхилення видалення використаної operation/fabric/accessory та read-модель сторінки товару.
- Ризики: cross-module consistency та зміна `Min` у довіднику після прив’язування.
