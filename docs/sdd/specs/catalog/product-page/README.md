# Товар і сторінка товару

**Модуль:** catalog  
**Статус:** у роботі  
**Власник:** Catalog  
**Створено:** 2026-07-27  
**Пов’язано:** `Product`, `ProductPhoto`, `ProductCategoryReference`, `Reference.Domain/GarmentPartOperation`

Спільні терміни визначені у [глосарії продукту](../../../../product/glossary.md).

## Мета

Як менеджер каталогу, я хочу створювати та надалі розширювати товар із українською/російською назвою, slug, типом, фото й категоріями, щоб API сторінки товару мав коректну доменну основу.

Для товару типу `Sewing` я хочу обирати операції пошиття, щоб система обчислювала трудомісткість і повне дробове значення кількості виробів за зміну без округлення.

## Межі поточної реалізації

- Реалізовано в Domain: `ProductId`, `ProductSlug`, `ProductType`, назви `Name`/`RuName`, `ProductPhoto`, `ProductCategoryReference`; методи create/update, add/remove/replace categories, керування фото.
- Реалізовано в Infrastructure: EF mapping `Products`, owned collections `ProductPhotos` і `ProductCategoryLinks`, FK категорії з `Restrict`.
- Не реалізовано: Product Application commands/queries, Product API-контракти та controllers, read-model сторінки товару, міграція/ручна API-перевірка.

## Модель за типом товару

`ProductType` має два значення: `Sewing` і `Ppe`.

### Спільна частина для обох типів

- українська назва `Name` та російська назва `RuName`;
- `Slug` і `ProductType`;
- фото через `ProductPhoto` → готовий `MediaFile`;
- категорії через `ProductCategoryReference` → `ProductCategoryId`;
- описовий блок, який буде деталізований наступним рішенням: **опис**, **інформація**, **характеристики**.

### Sewing: операції та трудомісткість

Для `ProductType.Sewing` Product зберігатиме вибрані посилання на `GarmentPartOperationId` із модуля Reference. Користувач додає або видаляє операції; самі `GarmentPartOperation` належать Reference і містять назву, частину виробу та `Min` — тривалість операції у хвилинах.

Sewing-товар також має одне ручне поле `MetersPerProduct`: кількість метрів тканини, потрібна на один виріб. Це властивість самого Product, а не окремої тканини. Поле обов’язкове для Sewing-товару, має бути більшим за нуль і не застосовується до `Ppe`.

Перелік тканин, перелік фурнітури, `MetersPerProduct` і трудові операції є виключно даними `ProductType.Sewing`. `Ppe` не може містити жодної з цих колекцій або полів.

### Ppe: постачальник

Для `ProductType.Ppe` Product має рівно одного постачальника через `SupplierId` із модуля Reference:

```text
Ppe Product
└── SupplierId → Reference.Supplier
```

`Supplier` належить Reference і містить назву та контактні дані. Product зберігає лише `SupplierId`; назва й контакти не копіюються. Для Ppe постачальник є обов’язковим, а для Sewing він не застосовується. Application перевіряє існування `SupplierId` у Reference перед збереженням.

### Ppe: коефіцієнт

Ppe-товар має один коефіцієнт у двох взаємовиключних режимах:

```text
PpeCoefficient
├── Source = Reference
│   └── AdditionalReferenceId → Reference.AdditionalReference (Unit = "%")
└── Source = Custom
    └── CustomValue (ручне значення користувача)
```

У режимі `Reference` Product зберігає лише `AdditionalReferenceId`, а під час розрахунку завжди читає актуальне `AdditionalReference.Value`. Зміна довідникового коефіцієнта автоматично впливає на наступний розрахунок усіх Ppe-товарів, що на нього посилаються.

У режимі `Custom` Product зберігає лише власний `CustomValue`; користувач може змінювати його в картці товару. Це **не** створює `AdditionalReference`: ручний коефіцієнт належить одному товару й не має засмічувати спільний довідник. Якщо значення стане повторно використовуваним правилом для багатьох товарів, користувач окремо створює довідниковий `AdditionalReference` у модулі Reference, а потім обирає його.

### Інваріанти коефіцієнта Ppe

- Ppe має рівно один режим: `Reference` або `Custom`;
- у `Reference` задано валідний `AdditionalReferenceId`, а `CustomValue` відсутнє;
- у `Custom` задано невід’ємне ручне значення, а `AdditionalReferenceId` відсутній;
- вибраний довідниковий запис має одиницю `%`;
- Sewing-товар не має Ppe-коефіцієнта.

Запропонована формула:

```text
totalOperationMinutes = Σ GarmentPartOperation.Min для всіх операцій товару
piecesPerShift = 480 / (totalOperationMinutes × 1.25)
```

`480` — тривалість зміни в хвилинах, а `1.25` — фіксований коефіцієнт до сумарної тривалості операцій. `piecesPerShift` — єдиний показник кількості виробів за зміну, який передається повним дробовим числовим значенням без округлення. Окремі показники завершених або теоретичних виробів і залишку зміни не обчислюються. Формула не застосовується до `Ppe`.

Sewing-товар може існувати як чернетка без операцій: у цьому стані показники не обчислюються. `Min` не snapshot-ується в Product: розрахунок завжди використовує актуальні значення Reference.

## Інваріанти

- Назви обов’язкові, до 200 символів; slug валідований value object і є унікальним у Catalog; type має бути визначеним enum.
- Одна category не додається двічі; категорія прив’язана лише за `ProductCategoryId`.
- Одне `MediaFileId` не прикріплюється двічі; файл має бути `Ready`.
- Якщо фото існують, головним є щонайменше одне; після видалення головного обирається наступне за sort order/ID.

## План фаз

| Фаза | Статус | Результат |
| --- | --- | --- |
| [01 Domain](phases/01-domain.md) | у проєктуванні | модель товару за типом і інваріанти операцій Sewing |
| [02 Infrastructure](phases/02-infrastructure.md) | у проєктуванні | цільова схема, EF mapping і міграції |
| Application | заплановано | CRUD, page query, validators, specs |
| API | заплановано | admin/public endpoint-и та contracts |
| Swagger | заплановано | ручні сценарії |
| Frontend handoff | заплановано | контракт сторінки товару |

## Ризики та відкриті питання

- Визначити, чи може товар належати кільком категоріям для storefront-фільтрації.
- Визначити публічну модель сторінки: опис, ціна, варіанти, наявність, SEO та локалізація ще не належать цій реалізації.
- Перед API реалізацією визначити авторизацію write-операцій, slug uniqueness, pagination/filtering і правила видалення пов’язаних media.
- Зміна `Min` у Reference, ціни тканини/фурнітури або `AdditionalReference` змінює наступні розрахунки товару; історія виробничих норм і цін не входить до поточного delivery.
- Видалення операції має прибирати links із Product через ідемпотентну міжмодульну подію; необхідно визначити delivery/retry та observability.

## Прийнята структура описових даних

### Description

Користувач заповнює два обов’язкові тексти: український та російський, до `20 000` символів кожен. Опис зберігається й передається як Markdown. Дозволені абзаци, заголовки, нумеровані та марковані списки, посилання, жирний текст і курсив. Таблиці, зображення та raw HTML не дозволені: таблиці задаються лише через Characteristics, а frontend рендерить Markdown через санітизований renderer.

### Information

Information — впорядкована колекція локалізованих блоків `заголовок → текст`. Кожен блок має українські й російські заголовок до `200` символів і plain-text текст до `2 000` символів. Це не таблиця характеристик: frontend показує кожен блок окремою інформаційною секцією обраною мовою.

### Characteristics

Characteristics — впорядкована колекція локалізованих підтаблиць. Кожна підтаблиця має український і російський заголовки до `200` символів та впорядкований список рядків з українськими й російськими `параметр → значення`. Параметр обмежений `200`, а значення — `1 000` символами:

```text
Characteristics
├── Таблиця «Матеріали»
│   ├── «Основна тканина» → «..."
│   └── «Щільність»       → «..."
└── Таблиця «Догляд»
    └── «Прання»          → «..."
```

Порядок таблиць і рядків є частиною даних, бо його використовує frontend. Максимальні розміри ще потрібно визначити.

## Перелік тканин

Product матиме впорядкований перелік посилань на `FabricId` із модуля Reference. До двох тканин позначаються як **основні**, усі інші — додаткові. Основні тканини завжди відображаються першими, а в межах кожної групи порядок визначає `SortOrder`. Кількість додаткових тканин не обмежена бізнес-моделлю: товар може мати дві, п’ять, десять або більше тканин.

```text
Product
└── Fabrics[]
    ├── FabricId → Reference.Fabric
    ├── IsPrimary
    └── SortOrder
```

`Fabric` належить Reference та зараз містить `Name`, поточну `Price` і `ProviderId`. Product зберігатиме лише ID та властивості власного зв’язку; не копіюватиме назву або ціну тканини.

### Прийняті інваріанти

- одна й та сама `FabricId` не додається двічі до одного Product;
- у товару з непорожнім переліком тканин є не більше двох основних тканин; якщо тканина лише одна, вона є основною;
- додаткових тканин може бути довільна кількість;
- перелік тканин доступний лише для `Sewing`; `Ppe` не може мати жодної тканини;
- перед збереженням Application перевіряє, що кожен `FabricId` існує в Reference.

### Ціноутворення — поза поточним рішенням

Ціна товару залежить від вибраних тканин. Формула трьох цінових діапазонів 1–10, 11–39 та 40+ шт. для кожної тканини зафіксована в [актуальній специфікації ціноутворення](../001-product-page/requirements/pricing.md). Вона обчислюється як read-model і не фіксується як snapshot у моделі даних.

Під час видалення Fabric із Reference потрібне таке саме подієве видалення links із Product, як для операцій Sewing; деталі контракту події буде визначено в Application/Integration фазі.

## Цінові рівні товару

Ціни є різними для двох типів Product. Формула Sewing описана в [актуальній специфікації ціноутворення](../001-product-page/requirements/pricing.md).

### Sewing: три ціни на основі тканин

Sewing-товар має три цінові рівні. Вони залежать від вибраних тканин: кожна тканина надає власні три ціни, які беруть участь у розрахунку ціни Product з урахуванням `MetersPerProduct`.

```text
Fabric
├── Sewing price level 1
├── Sewing price level 2
└── Sewing price level 3
       ↓ разом із MetersPerProduct
Product.Sewing price levels 1–3
```

Затверджені непересічні діапазони Sewing:

| Рівень | Кількість |
| --- | --- |
| 1 | 1–10 шт. |
| 2 | 11–39 шт. |
| 3 | 40+ шт. |

Результат є обчислюваним read-model для кожної пари «Product — Fabric»; окремі поля цін не створюються.

### Ppe: дві ціни

Ppe-товар має два цінові рівні: роздрібний та оптовий.

```text
Ppe Product
├── Retail price
└── Wholesale price
```

Затверджені непересічні діапазони Ppe:

| Рівень | Кількість |
| --- | --- |
| Роздріб | 1–9 шт. |
| Опт | 10+ шт. |

Формула Ppe-ціни з постачальником і коефіцієнтом буде додана окремо.

## Прийнята стратегія зберігання

Використовується composition-модель: спільна таблиця Product із discriminator `Type` та окремі таблиці деталей для кожного типу. Це не дві незалежні сутності товару і не одна широка таблиця з великою кількістю nullable-полів.

```text
catalog.Products
├── Id, Name, RuName, Slug, Type, DescriptionUk, DescriptionRu
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

Ціни Sewing є обчислюваним read-model для кожної пари «Product — Fabric» і не зберігаються окремими таблицями: формула використовує актуальні ціни Reference та `AdditionalReference`. Поточні `Products`, `ProductPhotos` і `ProductCategoryLinks` уже існують; всі таблиці Sewing/Ppe у схемі вище — цільові та ще не реалізовані.

### Правила цілісності

- `Products.Type` визначає, яка detail-модель дозволена.
- Sewing має рівно один `SewingProductDetails` і не має `PpeProductDetails`.
- Ppe має рівно один `PpeProductDetails` і не має жодної Sewing detail/collection.
- `SewingProductDetails.ProductId` та `PpeProductDetails.ProductId` є одночасно PK і FK: не може існувати більш ніж один запис деталей на товар.
- Перевірки `Type` і взаємовиключних полів залишаються у Domain/Application; БД забезпечує ключі, FK та унікальність зв’язків.

## Перелік фурнітури

Product матиме впорядкований перелік посилань на `GarmentAccessoryId` із модуля Reference. Для кожної вибраної фурнітури користувач задає кількість, потрібну для одного виробу.

```text
Product
└── Accessories[]
    ├── GarmentAccessoryId → Reference.GarmentAccessory
    ├── Quantity
    └── SortOrder
```

`GarmentAccessory` належить Reference та зараз містить `Name`, поточну `Price` і `SupplierId`. Product зберігатиме лише ID, кількість і порядок власного зв’язку; не копіюватиме ціну або назву фурнітури.

### Прийняті інваріанти

- одна й та сама `GarmentAccessoryId` не додається двічі до одного Product;
- кількість є обов’язковою та має бути більшою за нуль;
- Application перевіряє існування кожного `GarmentAccessoryId` у Reference перед збереженням;
- перелік фурнітури доступний лише для `Sewing`; `Ppe` не може мати жодної фурнітури.
- `Ppe` має рівно один валідний `SupplierId`; `Sewing` не має постачальника на рівні Product.
- `Ppe` має рівно один коефіцієнт у режимі Reference або Custom; `Sewing` не має Ppe-коефіцієнта.

### Ціноутворення — поза поточним рішенням

Вартість фурнітури впливає на ціну товару: кількість для виробу множиться на актуальну ціну фурнітури та входить до базової собівартості за [формулою Sewing](../001-product-page/requirements/pricing.md). Дробові результати не округлюються.

Під час видалення `GarmentAccessory` із Reference його links мають бути прибрані з Product через ідемпотентну міжмодульну подію.

## Журнал змін

- 2026-07-27 — Зафіксовано поточну доменну та persistence основу; цикл свідомо лишається відкритим для розширення.
- 2026-07-27 — Додано SDD-проєктування Sewing-операцій і розрахунку трудомісткості.
- 2026-07-27 — Прийнято: 480 хвилин за зміну, `floor` для повних виробів, чернетка без операцій, актуальний `Min` без snapshot і подієве видалення links після видалення довідникової операції.
- 2026-07-27 — Визначено Description (два Markdown-тексти), Information (заголовок → текст) і Characteristics (підтаблиці з рядками параметр → значення).
- 2026-07-27 — Додано SDD-проєктування переліку тканин: одна основна, необмежені додаткові; ціноутворення відкладено до уточнення правил.
- 2026-07-27 — Додано SDD-проєктування переліку фурнітури з кількістю на один виріб; ціноутворення відкладено до уточнення правил.
- 2026-07-27 — Додано ручне product-level поле `MetersPerProduct` для Sewing-товару.
- 2026-07-27 — Уточнено: тканини, фурнітура, метри на виріб і трудові операції належать лише Sewing-товару.
- 2026-07-27 — Додано Ppe-специфічний обов’язковий `SupplierId`.
- 2026-07-27 — Додано Ppe-коефіцієнт: посилання на актуальний AdditionalReference або локальне ручне значення.
- 2026-08-07 — Зафіксовано формулу ціни Sewing, що утворює три рівні для кожної вибраної тканини з актуальних цін Reference та коефіцієнтів `AdditionalReference`; Ppe має два рівні роздріб/опт.
- 2026-07-27 — Прийнято composition persistence-стратегію: Products + окремі Sewing/Ppe details і Sewing links.
