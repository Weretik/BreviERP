# BreviERP API — інструкція інтеграції frontend

Версійована точка входу OpenAPI — [openapi.yaml](openapi.yaml). Генеруйте типи й clients із цього файла; не копіюйте DTO з backend source. Агрегований контракт наразі містить лише API, для яких уже створено та перевірено окремий versioned contract. Відсутність endpoint у цьому файлі означає прогалину contract coverage, а не відсутність runtime endpoint.

## Authentication

`POST /api/auth/session/login` повертає opaque Bearer access token і встановлює дві cookies: HTTP-only refresh cookie `kedr.rt`, яка надсилається лише на refresh route, та доступну для читання CSRF cookie `kedr.csrf`. Ці назви є поточною legacy-конфігурацією BreviERP; frontend має використовувати їх як runtime contract, доки окрема міграція не змінить назви.

Для захищених операцій надсилайте `Authorization: Bearer <accessToken>`. Щоб поновити сесію, викликайте `POST /api/auth/session/refresh` із `credentials: 'include'` і передавайте в header `X-CSRF-Token` поточне значення cookie `kedr.csrf`. Refresh endpoint обертає обидві cookies і повертає новий access token. `POST /api/auth/session/logout` також потребує Bearer authentication і видаляє обидві cookies. `GET /api/auth/session/me` повертає поточного authenticated користувача.

Host використовує authenticated fallback policy. Публічними є лише endpoints, явно позначені `AllowAnonymous` у controller.

## HTTP-конвенції

- JSON використовує camelCase-назви properties ASP.NET Core.
- Paginated responses використовують `{ pagedInfo, value }`. `pagedInfo` містить `pageNumber`, `pageSize`, `totalPages` і `totalRecords`; `value` містить масив рядків.
- Validation failures повертають `400` у чинному форматі Ardalis validation errors. Не покладайтеся на локалізований текст повідомлення.
- `404` не має response body. `409` повертає масив conflict messages. `401` означає відсутню або недійсну authentication, а `403` — authenticated користувача без потрібного доступу.
- Decimal values передаються як JSON numbers. IDs є integers, якщо schema явно не визначає `uuid` або `string`.

## Поточна карта API

| Область | Route | Доступ | OpenAPI coverage |
| --- | --- | --- | --- |
| Products | `/api/v1/products`, `/api/v1/products/{id}` | Anonymous у поточному controller | [Catalog product contract](catalog/product-catalog.openapi.yaml) |
| Product categories | `/api/reference/product-categories/...` | Anonymous у поточному controller | ще не додано |
| Catalog media | `/api/catalog/media/...` | Anonymous у поточному controller | ще не додано |
| Suppliers | `/api/reference/suppliers` | Anonymous у поточному controller | [Reference suppliers contract](reference/suppliers.openapi.yaml) |
| Other reference data | `/api/reference/fabrics`, `/garment-parts`, `/garment-accessories`, `/garment-part-operations`, `/additional-references` | Anonymous у поточних controllers | ще не додано |
| Session | `/api/auth/session/login`, `/refresh`, `/logout`, `/me` | login/refresh — Anonymous; logout/me — Bearer | ще не додано |

Перед використанням endpoint без versioned OpenAPI contract frontend і backend мають погодити request, response, status codes і authorization та додати контракт до [агрегованого OpenAPI](openapi.yaml).
