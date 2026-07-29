# Product page — API contract

**Статус:** чернетка; HTTP-контракт ще не погоджено.

## Призначення

Цей документ фіксує межі майбутнього контракту. Після виконання T005 машинним джерелом правди для frontend стане `contracts/product-catalog.openapi.yaml`, а цей файл міститиме лише рішення щодо аудиторії, версіонування та сумісності.

Swagger UI не є контрактом: це лише інтерфейс для перегляду OpenAPI. Frontend може працювати без нього, використовуючи versioned OpenAPI YAML/JSON для генерації типів і клієнта, contract testing та локальної документації.

## Що потрібно погодити до API-коду

- admin CRUD Product і порядок оновлення type-specific даних;
- public read-model сторінки товару;
- маршрути, HTTP methods, operation IDs і версіонування;
- request/response DTO, nullable-поля, paging/filtering і приклади;
- авторизацію write-операцій та `Idempotency-Key`, якщо write-сценарій створює ризик дублювання;
- стабільний формат validation/error response;
- slug uniqueness, правила видалення media та локалізацію описових даних.

## Правило handoff

До передачі frontend `product-catalog.openapi.yaml` має описувати кожний доступний endpoint, параметри, request body, усі response-коди, security scheme, помилки та приклади. Зміна публічного контракту потребує оцінки сумісності й оновлення frontend-клієнта.

