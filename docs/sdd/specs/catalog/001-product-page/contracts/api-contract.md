# Product page — API contract

**Статус:** погоджено для фази 00. Машинне джерело правди: [product-catalog.openapi.yaml](product-catalog.openapi.yaml).

**Версіонування:** усі endpoints цього delivery мають префікс `/api/v1`.

## Призначення

Цей документ фіксує межі майбутнього контракту. Після виконання T005 машинним джерелом правди для frontend стане `contracts/product-catalog.openapi.yaml`, а цей файл міститиме лише рішення щодо аудиторії, версіонування та сумісності.

Swagger UI не є контрактом: це лише інтерфейс для перегляду OpenAPI. Frontend може працювати без нього, використовуючи versioned OpenAPI YAML/JSON для генерації типів і клієнта, contract testing та локальної документації.

## Прийнятий scope

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

Admin list залишається компактним і передає лише `categoryIds`, `minimumWholesalePrice` та `mainPhoto` як `null` або об’єкт `mediaFileId` і готового `url`. Натомість повний admin detail (`GET /{id}` та успішні `POST`/`PUT`) одним response розкриває категорії об’єктами `id`, `name`, `ruName`, `slug`.

Фото передається об’єктом із `mediaFileId` і готовим `url`. У повному admin detail для кожного фото додатково передаються `alt`, `isVisible`, `isMain` і `sortOrder`; у списку — лише головне фото.

Create/Update request передає лише IDs вибраних Reference-сутностей і ручні значення Product. Повний admin detail додатково розкриває вибрані довідникові дані, потрібні для відображення: ID, назву та поточну ціну тканини/фурнітури, ID, назву й поточну кількість хвилин операції, ID і назву постачальника, а для вибраного `AdditionalReference` — ID, назву, ключ, `value` та `unit`.

Кожний рядок admin-списку містить лише `id`, українську назву, `slug`, `type`, `categoryIds`, `minimumWholesalePrice`, головне фото з URL або `null`, `createdAtUtc` та `updatedAtUtc`. Повний admin detail повертається лише для `GET /{id}`, а також успішних `POST` і `PUT`; він є готовою проєкцією для форми й містить розгорнуті категорії, усі фото з URL, вибрані Reference-сутності та всі застосовні ціни. Frontend не виконує окремі запити за кожним ID зі складу одного Product.

Public storefront endpoint, його SEO, наявність, варіанти та поведінка локалізації не належать поточному delivery.

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

У request і response поле `type` є рядковим enum: лише `Sewing` або `Ppe`; числове представлення .NET enum не є частиною HTTP contract. Create і full-replace `PUT` приймають це поле, а list/detail відповіді завжди його повертають.

ID для `POST` вводить адміністратор у request body. Він має бути додатним і унікальним. За повторного ID API повертає `409 Conflict`; `PUT` бере ID тільки з route `{id}` і не приймає його в body.

Успішне створення повертає `201 Created` і повний admin detail створеного Product. Читання й повне оновлення повертають `200 OK` і повний актуальний admin detail; це дає frontend усі розрахунки без додаткового `GET`. Видалення повертає `204 No Content`. Відсутній Product повертає `404 Not Found`, невалідний request — `400 Bad Request`, а повторний ID, українська/російська назва або відхилене видалення зв’язаної сутності — `409 Conflict`.

Наявний `ResultToActionResult` already maps `Ardalis.Result.Invalid`, `NotFound` і `Conflict` у `400`, `404` і `409`. До фази API його потрібно розширити або виконати еквівалентне HTTP-мапування в controller для `201 Created` і `204 No Content`, не змінюючи погоджений контракт.

Новий error envelope не створюється. API використовує поточний формат `Ardalis.Result`: для `400` — його validation errors, для `409` — його errors, для `404` — порожнє тіло відповіді.

## Що потрібно погодити до API-коду

- остаточну authorization policy перед production;

## Прийнята коригувальна зміна

2026-08-19 — `03.8-catalog-read-model-enrichment` надав Application
read-model з URL головного/усіх фото та розрахованими PPE цінами. Після цього
`04.2` створює API DTO/mappers, а `04.3` синхронізує OpenAPI YAML. До завершення
цих підфаз контракт не передається frontend як реалізований.

## Правило handoff

До передачі frontend `product-catalog.openapi.yaml` описує кожний доступний endpoint, параметри, request body, усі response-коди, помилки та приклади. Зміна публічного контракту потребує оцінки сумісності й оновлення frontend-клієнта.
