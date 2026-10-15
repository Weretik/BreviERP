# Сторінка товару — API-контракт

**Статус:** реалізовано та перевірено automated tests; ручні Swagger/API сценарії лишаються відкритими. Машинне джерело правди: [product-catalog.openapi.yaml](../../../../contracts/catalog/product-catalog.openapi.yaml).

**Версіонування:** усі endpoints цього delivery мають префікс `/api/v1`.

## Призначення

Цей документ фіксує межі контракту. Машинним джерелом правди для frontend є `docs/sdd/contracts/catalog/product-catalog.openapi.yaml`, а цей файл містить рішення щодо аудиторії, версіонування та сумісності.

Swagger UI не є контрактом: це лише інтерфейс для перегляду OpenAPI. Frontend може працювати без нього, використовуючи versioned OpenAPI YAML/JSON для генерації типів і клієнта, contract testing та локальної документації.

## Прийняті межі

Поточний HTTP-контракт призначений лише для admin frontend. Admin detail/read-model має передавати всі дані, які менеджер може налаштувати:

- спільні поля Product, фото та категорії;
- локалізовані `Description`, `Information` і `Characteristics`;
- для Sewing: `MetersPerProduct`, тканини з ознакою основної та порядком, фурнітуру, операції, `piecesPerShift`, три ціни для кожної тканини й min/max для кожного цінового діапазону;
- для PPE: `SupplierId`, `BasePrice`, а також роздрібний і оптовий відсотки — кожен із режимом та відповідно `AdditionalReferenceId` або ручним значенням.

Admin list повертається як `Ardalis.Result.PagedResult<IReadOnlyList<ProductListItem>>`: елементи лежать у `value`, а metadata (`pageNumber`, `pageSize`, `totalPages`, `totalRecords`) — у `pagedInfo`. Кожен елемент містить `minimumWholesalePrice`: для Sewing — мінімум усіх наявних розрахованих цін усіх тканин і діапазонів, для PPE — обчислену оптову ціну, а за неможливого розрахунку — `0`. Він підтримує:

- єдиний пошук за ID, українською та російською назвами; `Slug` не входить до пошуку. Один числовий token шукає ID точно, а текст ділиться за пробілами: кожен token шукається частково, без урахування регістру й має збігтися хоча б з однією локалізованою назвою;
- фільтр за `ProductType`;
- фільтр за однією категорією: товари з обраної категорії та всіх її дочірніх категорій;
- сортування за `id`, `name`, `createdAt` або `updatedAt` у напрямку `asc`/`desc`.

Параметр сторінки має значення від 1. `pageSize` за замовчуванням дорівнює 20 і може мати лише значення 10, 20 або 50. Сортування за замовчуванням: `sortBy=name`, `sortDirection=asc`.

Адміністративний список залишається компактним і передає лише `categoryIds`, `minimumWholesalePrice` та `mainPhoto` як `null` або об’єкт `mediaFileId` і готового `url`. Натомість повні адміністративні деталі (`GET /{id}` та успішні `POST`/`PUT`) в одній відповіді розкривають категорії об’єктами `id`, `name`, `ruName`, `slug`.

Фото передається об’єктом із `mediaFileId` і готовим `url`. У повному admin detail для кожного фото додатково передаються `alt`, `isVisible`, `isMain` і `sortOrder`; у списку — лише головне фото.

Запит створення/оновлення передає лише ID вибраних сутностей Reference і вручну задані значення Product. Повні адміністративні деталі додатково розкривають вибрані довідникові дані, потрібні для відображення: ID, назву та поточну ціну тканини/фурнітури, ID, назву й поточну кількість хвилин операції, ID і назву постачальника, а для вибраного `AdditionalReference` — ID, назву, ключ, `value` та `unit`.

Кожний рядок admin-списку містить лише `id`, українську назву, `slug`, `type`, `categoryIds`, `minimumWholesalePrice`, головне фото з URL або `null`, `createdAtUtc` та `updatedAtUtc`. Повний admin detail повертається лише для `GET /{id}`, а також успішних `POST` і `PUT`; він є готовою проєкцією для форми й містить розгорнуті категорії, усі фото з URL, вибрані Reference-сутності та всі застосовні ціни. Frontend не виконує окремі запити за кожним ID зі складу одного Product.

Публічний endpoint вітрини, його SEO, наявність, варіанти та поведінка локалізації не належать поточному постачанню.

Тимчасово всі admin endpoints мають явний `AllowAnonymous`, попри fallback authentication policy Host. Перед production цей доступ має бути замінений окремо погодженою authorization policy; анонімний доступ не є production-рішенням.

`Idempotency-Key` не входить до поточного контракту create/update. Admin frontend має блокувати повторне надсилання форми, а update звертається до наявного `ProductId`.

Поточний delivery містить повний CRUD Product. Базові маршрути:

- `GET /api/v1/products` — сторінковий список;
- `POST /api/v1/products` — створення;
- `GET /api/v1/products/{id}` — повний admin detail;
- `PUT /api/v1/products/{id}` — редагування;
- `DELETE /api/v1/products/{id}` — повне видалення Product.

`DELETE` прибирає Product разом із його власними detail-записами та links. `MediaFile` не видаляється й лишається доступним у модулі Media.

`PUT` приймає повний стан усіх редагованих даних Product: спільні поля, фото, категорії, контент і дані рівно одного типу (`Sewing` або `Ppe`). Часткових endpoint-ів для цих частин немає. Якщо тип змінюється, попередні type-specific details і links видаляються, а дані нового типу зберігаються з надісланого body. `Slug` не передається: backend генерує його з української назви через пакет транслітерації.

У запиті й відповіді поле `type` є рядковим переліком: лише `Sewing` або `Ppe`; числове представлення переліку .NET не є частиною HTTP-контракту. Створення й повна заміна через `PUT` приймають це поле, а відповіді списку/деталей завжди його повертають.

ID для `POST` вводить адміністратор у тілі запиту. Він має бути додатним і унікальним. За повторного ID API повертає `409 Conflict`; `PUT` бере ID тільки з маршруту `{id}` і не приймає його в тілі запиту.

Успішне створення повертає `201 Created` і повні адміністративні деталі створеного Product. Читання й повне оновлення повертають `200 OK` і повні актуальні адміністративні деталі; це дає frontend усі розрахунки без додаткового `GET`. Видалення повертає `204 No Content`. Відсутній Product повертає `404 Not Found`, невалідний запит — `400 Bad Request`, а повторний ID, українська/російська назва або відхилене видалення зв’язаної сутності — `409 Conflict`.

`ResultToActionResult` мапить `Ardalis.Result.Invalid`, `NotFound` і `Conflict` у `400`, `404` і `409`. `ProductsController` явно повертає `201 Created` для create та `204 No Content` для delete, без зміни погодженого контракту.

Новий error envelope не створюється. API використовує поточний формат `Ardalis.Result`: для `400` — його validation errors, для `409` — його errors, для `404` — порожнє тіло відповіді.

Для `POST` і `PUT` boundary перевіряє string enum до виклику Mediator. Допустимі лише case-sensitive значення `type: "Sewing" | "Ppe"` та `retailPercent.source` / `wholesalePercent.source: "Reference" | "Custom"`; числові, string-numeric, case-variant і невідомі значення є невалідними. Помилка має той самий масив `ValidationError`, що й Application validation, наприклад:

```json
[
  {
    "identifier": "type",
    "errorMessage": "type must be either Sewing or Ppe."
  }
]
```

або для PPE-відсотка:

```json
[
  {
    "identifier": "retailPercent.source",
    "errorMessage": "retailPercent.source must be either Reference or Custom."
  }
]
```

## Що потрібно погодити до API-коду

- остаточну authorization policy перед production;

## Прийнята коригувальна зміна

2026-08-19 — `03.8-catalog-read-model-enrichment` надав Application
read-model з URL головного/усіх фото та розрахованими PPE цінами. Після цього
`04.2` створює API DTO/mappers, а `04.3` синхронізує OpenAPI YAML. До завершення
цих підфаз контракт не передається frontend як реалізований.

## Правило передачі

До передачі frontend `product-catalog.openapi.yaml` описує кожну доступну кінцеву точку, параметри, тіло запиту, усі коди відповіді, помилки та приклади. Зміна публічного контракту потребує оцінки сумісності й оновлення frontend-клієнта.
