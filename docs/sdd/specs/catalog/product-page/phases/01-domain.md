# Фаза 01: Доменна модель товару за типом

**Статус:** у проєктуванні  
**Залежить від:** прийнятого рішення щодо правил Sewing  
**Блокує:** persistence, Application та API для операцій

## Результат

`Product` може представити спільні дані двох типів товару й набір операцій тільки для `ProductType.Sewing`, не порушуючи межі Catalog та Reference.

## Наявна основа

`Product` уже є aggregate root. Він володіє назвами, slug, type, `ProductPhoto` і `ProductCategoryReference`. `ProductPhoto` посилається на `MediaFileId` і перевіряється aggregate-ом на готовність медіафайлу.

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

## Синхронізація з довідником Reference

Коли `GarmentPartOperation` видаляється, усі `SewingOperations` із цим ID мають бути видалені з Product. Це не прямий EF cascade: Catalog і Reference мають різні модульні DbContext-и та власників даних.

Цільовий механізм: після успішного видалення операції Reference публікує подію `GarmentPartOperationDeleted`; Catalog обробляє її окремим handler-ом, знаходить Product із цим ID і видаляє відповідні links. Доставка має бути ідемпотентною: повторна подія без наявного link не є помилкою. Синхронізація може бути eventually consistent.

## Потрібні рішення перед кодом

1. Контракт події видалення, delivery/retry та observability.
3. Максимальні розміри Description, Information і Characteristics.

## Критерії приймання для реалізації

- [ ] `Ppe` не може мати Sewing-операції.
- [ ] Одна операція не додається двічі до одного Product.
- [ ] Некоректний або відсутній operation ID не потрапляє до aggregate-а.
- [ ] Sewing-чернетка без операцій дозволена; трудомісткість для неї не обчислюється.
- [ ] Трудомісткість не ділить на нуль; `piecesPerShift` передається повним дробовим значенням без округлення.
- [ ] Зміна `Min` у Reference змінює наступний розрахунок без зміни Product.
- [ ] Видалення операції прибирає її links із Product через ідемпотентну міжмодульну подію.
- [ ] Catalog не отримує залежність від Reference Infrastructure або entity.

## Тканини: доповнення до доменної моделі

`Fabrics` буде дочірньою колекцією Product із полями `FabricId`, `IsPrimary` та `SortOrder`. Вона не містить entity `Fabric`, її `Price` або `ProviderId`. Product забезпечує унікальність тканини й не більше двох основних тканин; якщо в списку лише одна тканина, вона є основною. Read-model відображає основні тканини першими, а потім застосовує `SortOrder`. Application перевіряє існування тканин через Reference abstraction.

`MetersPerProduct` є одним ручним додатним полем самого Sewing-товару: кількістю метрів тканини на один виріб. Воно не належить `Fabrics[]`, не дублюється для кожної тканини та не застосовується до `Ppe`.

Витрату визначено як product-level `MetersPerProduct`. Правила розрахунку трьох цінових діапазонів 1–10 / 11–39 / 40+ шт. для кожної вибраної тканини визначені в [актуальній специфікації ціноутворення](../../001-product-page/requirements/pricing.md). Результат є обчислюваним read-model, а не snapshot у Product: він використовує актуальні ціни Reference і `AdditionalReference`. Видалення Fabric має очистити links із Product через ідемпотентну міжмодульну подію.

Для Ppe потрібні два цінові рівні — роздрібний та оптовий — і окрема формула, що застосовує вибраний постачальник та Ppe-коефіцієнт. Межі кількості для обох типів мають бути непересічними та без пропусків до появи доменної моделі цін.

## Фурнітура: доповнення до доменної моделі

`Accessories` буде дочірньою колекцією Product із полями `GarmentAccessoryId`, `Quantity` та `SortOrder`. Вона не містить entity `GarmentAccessory`, його `Price` або `SupplierId`. Product забезпечує унікальність фурнітури та додатну кількість; Application перевіряє існування ID через Reference abstraction.

Розрахунок вартості використовує актуальну ціну Reference і кількість на виріб відповідно до [формули Sewing](../../001-product-page/requirements/pricing.md); дробові результати не округлюються. Видалення фурнітури має очистити links із Product через ідемпотентну міжмодульну подію.

## Межа типу товару

`Fabrics`, `Accessories`, `MetersPerProduct` і `SewingOperations` доступні лише для `ProductType.Sewing`. Product із типом `Ppe` не може створювати, зберігати або змінювати ці дані.

`ProductType.Ppe` має рівно один обов’язковий `SupplierId`, що посилається на `Reference.Supplier`. Product не містить entity Supplier; Application перевіряє існування ID через Reference abstraction. Sewing-товар не може мати `SupplierId` на рівні Product.

Ppe також має value object `PpeCoefficient` із двома взаємовиключними станами: `Reference(AdditionalReferenceId)` або `Custom(decimal value)`. У першому випадку Application читає актуальне значення довідника Reference з unit `%`; у другому значення належить Product і є невід’ємним. Product не створює AdditionalReference автоматично. Sewing-товар не може мати `PpeCoefficient`.

## Перевірка

- Unit-тести: add/remove/replace operations, duplicate ID, type boundary, zero total, calculation without rounding.
- Integration-тести: перевірка наявності Reference operation, видалення operation та read-модель сторінки товару.
- Ризики: cross-module consistency та зміна `Min` у довіднику після прив’язування.
