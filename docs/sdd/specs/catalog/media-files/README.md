# Медіафайли каталогу

**Модуль:** catalog  
**Статус:** перевірено  
**Власник:** Catalog  
**Створено:** 2026-07-27  
**Пов’язано:** `CatalogMediaController`, `MediaFile`, `IMediaStorageService`

## Мета

Як користувач API каталогу, я хочу завантажувати, переглядати та видаляти зображення, щоб готові медіафайли можна було безпечно прив’язувати до товарів.

## Межі

- У межах: JPEG/PNG/WebP, метадані та статус медіафайлу, upload/list/delete API, зовнішнє сховище `adm.tools`.
- Поза межами: авторизація endpoint-ів (поточний `AllowAnonymous` погоджено окремо), обробка/ресайз зображень, прив’язування фото до Product.
- Припущення: API сховища доступний, credentials передаються лише конфігурацією.
- Залежності: PostgreSQL, `AdmToolsMediaStorageService`, `CatalogMediaUpload` і `AdmToolsStorage` options.

## Сценарії

1. За умови валідного JPEG/PNG/WebP, коли клієнт надсилає multipart upload, тоді файл зберігається, `MediaFile` стає `Ready`, а клієнт отримує ID і public URL.
2. За умови невідповідності MIME type та сигнатури, коли клієнт завантажує файл, тоді handler не викликається й повертається validation result.
3. За умови наявного файлу, коли клієнт видаляє його за ID, тоді об’єкт видаляється зі сховища й БД.

## Контракт і сумісність

- API contract: [contracts/api-contract.md](contracts/api-contract.md).
- Нові маршрути: `GET|POST /api/catalog/media`, `DELETE /api/catalog/media/{id}`.
- Авторизація: endpoint-и наразі явно `AllowAnonymous`; це свідомий тимчасовий контракт.
- Ідемпотентність: upload не підтримує `Idempotency-Key`; повторний upload створює окремий файл. Delete повертає `NotFound` для відсутнього ID.

## Фази реалізації

| Фаза | Статус | Потрібна? | Результат |
| --- | --- | --- | --- |
| [01 Domain](phases/01-domain.md) | перевірено | так | `MediaFile`, ID, статуси й інваріанти |
| [02 Infrastructure](phases/02-infrastructure.md) | перевірено | так | EF mapping та adm.tools adapter |
| [03 Application](phases/03-application.md) | перевірено | так | upload/list/delete CQRS і validation |
| [04 API](phases/04-api.md) | перевірено | так | multipart endpoint-и та Result mapping |
| [05 Swagger manual test](phases/05-swagger-manual-test.md) | заплановано | так | відтворювана ручна перевірка |
| [06 Frontend handoff](phases/06-frontend-handoff.md) | заплановано | так | стабільний контракт для сторінки товару |

## Файли

- Створено/змінено: `Catalog.Domain/Media`, `Catalog.Application/Features/Media`, `Catalog.Infrastructure/Services/AdmToolsMediaStorageService`, `Catalog.Api/Controllers/CatalogMediaController`.
- Не змінювати без окремої специфікації: секрети, deployment і глобальна serialisation-конфігурація.

## Критерії приймання

- [x] Допускаються лише JPEG, PNG і WebP з відповідною файловою сигнатурою.
- [x] Файл має пройти lifecycle `PendingUpload → Ready` перед використанням у Product.
- [x] Storage key унікальний; у БД для нього є унікальний індекс.
- [x] Upload/list/delete проходять через Mediator.
- [ ] API-відповіді та ручні Swagger-випадки перевірені на налаштованому сховищі.

## Перевірка

- Build: `dotnet build .\BreviERP.sln --no-restore` — успішно 2026-07-27.
- Tests: `dotnet test .\BreviERP.sln --no-build` — успішно, UnitTests 1/1, IntegrationTests 1/1.
- Manual: не виконано, потребує доступного `adm.tools` і тестових credentials.

## Ризики та відкриті питання

- Після успішного upload до сховища та до оновлення БД можливий orphaned object при технічному збої.
- Delete спочатку видаляє файл зі сховища, потім запис БД; потрібна recovery-стратегія для часткового збою.
- Перед production потрібно визначити access policy та retention для неприкріплених медіафайлів.

## Журнал змін

- 2026-07-27 — Зафіксовано реалізований media lifecycle і перенесено signature validation до Application.
