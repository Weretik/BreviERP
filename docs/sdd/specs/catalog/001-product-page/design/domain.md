# Product page — design: Domain

**Статус:** у проєктуванні  
**Залежить від:** прийнятого рішення щодо правил Sewing  
**Блокує:** persistence, Application та API для операцій

## Результат

`Product` може представити спільні дані двох типів товару й набір операцій тільки для `ProductType.Sewing`, не порушуючи межі Catalog та Reference.

## Наявна основа

`Product` уже є aggregate root. Він володіє назвами, slug, type, `ProductPhoto` і `ProductCategoryReference`. `ProductPhoto` посилається на `MediaFileId` і перевіряється aggregate-ом на готовність медіафайлу.

Поточна технічна база та правила інтеграції з Reference описані в [implementation-baseline.md](implementation-baseline.md).

## Запропоноване проєктування

```text
Catalog.Product (aggregate root)
├── спільні властивості: Name, RuName, Slug, Type, Photos, Categories
├── описовий блок: Description, Information, Characteristics      (ще не деталізовано)
└── SewingOperations[]                                             (лише для Type = Sewing)
    └── GarmentPartOperationId ──→ Reference.GarmentPartOperation
                                      └── Min
```

`SewingOperations` є дочірньою колекцією Product і містить тільки типізований ID довідникової операції. Product не містить `GarmentPartOperation` entity та не залежить від Reference Infrastructure. Application перевірить існування ID через абстракцію читання Reference.

## Прийнятий розрахунок

```text
totalOperationMinutes = Σ operation.Min
theoreticalPiecesPerShift = 480 / totalOperationMinutes
piecesPerShift = floor(480 / totalOperationMinutes)
remainingShiftMinutes = 480 - (piecesPerShift × totalOperationMinutes)
```

`480` — встановлена тривалість зміни у хвилинах. `piecesPerShift` містить лише повністю завершені вироби; теоретичне дробове значення потрібне для аналітики, а залишок показує невикористані хвилини.

Sewing-товар може існувати як чернетка без операцій; для нього показники не обчислюються. Розрахунок також не виконується, якщо сума `Min` дорівнює нулю. `Min` завжди читається з актуального `GarmentPartOperation` у Reference; Product не зберігає snapshot, тому зміна довідника змінює наступний розрахунок.

## Синхронізація з довідником Reference

Коли `GarmentPartOperation` видаляється, усі `SewingOperations` із цим ID мають бути видалені з Product. Це не прямий EF cascade: Catalog і Reference мають різні модульні DbContext-и та власників даних.

Цільовий механізм: після успішного видалення операції Reference публікує подію `GarmentPartOperationDeleted`; Catalog обробляє її окремим handler-ом, знаходить Product із цим ID і видаляє відповідні links. Доставка має бути ідемпотентною: повторна подія без наявного link не є помилкою. Синхронізація може бути eventually consistent.

## Потрібні рішення перед кодом

1. Контракт події видалення, delivery/retry та observability.
2. Чи потрібна історія розрахунків, якщо `Min` у довіднику змінюється.
3. Структура і валідація Description, Information, Characteristics.

## Критерії приймання для реалізації

- [ ] `Ppe` не може мати Sewing-операції.
- [ ] Одна операція не додається двічі до одного Product.
- [ ] Некоректний або відсутній operation ID не потрапляє до aggregate-а.
- [ ] Sewing-чернетка без операцій дозволена; трудомісткість для неї не обчислюється.
- [ ] Трудомісткість не ділить на нуль; `piecesPerShift` округлюється вниз.
- [ ] Зміна `Min` у Reference змінює наступний розрахунок без зміни Product.
- [ ] Видалення операції прибирає її links із Product через ідемпотентну міжмодульну подію.
- [ ] Catalog не отримує залежність від Reference Infrastructure або entity.

## Тканини: доповнення до доменної моделі

`Fabrics` буде дочірньою колекцією Product із полями `FabricId`, `IsPrimary` та `SortOrder`. Вона не містить entity `Fabric`, її `Price` або `ProviderId`. Product забезпечує унікальність тканини та правило рівно однієї основної тканини, якщо список не порожній; Application перевіряє існування тканин через Reference abstraction.

`MetersPerProduct` є одним ручним додатним полем самого Sewing-товару: кількістю метрів тканини на один виріб. Воно не належить `Fabrics[]`, не дублюється для кожної тканини та не застосовується до `Ppe`.

Ціноутворення не належить цій фазі до окремого рішення: витрату вже визначено як product-level `MetersPerProduct`, але потрібні правила трьох цінових діапазонів 1–10 / 11–39 / 40+ шт., валюти та округлення. Видалення Fabric має очистити links із Product через ідемпотентну міжмодульну подію.

Для Ppe потрібні два цінові рівні — роздрібний та оптовий — і окрема формула, що застосовує вибраний постачальник та Ppe-коефіцієнт. Межі кількості для обох типів мають бути непересічними та без пропусків до появи доменної моделі цін.

## Фурнітура: доповнення до доменної моделі

`Accessories` буде дочірньою колекцією Product із полями `GarmentAccessoryId`, `Quantity` та `SortOrder`. Вона не містить entity `GarmentAccessory`, його `Price` або `SupplierId`. Product забезпечує унікальність фурнітури та додатну кількість; Application перевіряє існування ID через Reference abstraction.

Майбутній розрахунок вартості використовує актуальну ціну Reference і кількість на виріб, але одиниця Quantity, правила округлення та партійне ціноутворення ще не визначені. Видалення фурнітури має очистити links із Product через ідемпотентну міжмодульну подію.

## Межа типу товару

`Fabrics`, `Accessories`, `MetersPerProduct` і `SewingOperations` доступні лише для `ProductType.Sewing`. Product із типом `Ppe` не може створювати, зберігати або змінювати ці дані.

`ProductType.Ppe` має рівно один обов’язковий `SupplierId`, що посилається на `Reference.Supplier`. Product не містить entity Supplier; Application перевіряє існування ID через Reference abstraction. Sewing-товар не може мати `SupplierId` на рівні Product.

Ppe також має value object `PpeCoefficient` із двома взаємовиключними станами: `Reference(AdditionalReferenceId)` або `Custom(decimal value)`. У першому випадку Application читає актуальне значення довідника Reference з unit `%`; у другому значення належить Product і є невід’ємним. Product не створює AdditionalReference автоматично. Sewing-товар не може мати `PpeCoefficient`.

## Перевірка

- Unit-тести: add/remove/replace operations, duplicate ID, type boundary, zero total, calculation/rounding.
- Integration-тести: перевірка наявності Reference operation, видалення operation та read-модель сторінки товару.
- Ризики: cross-module consistency та зміна `Min` у довіднику після прив’язування.
