# Зміна API: медіафайли каталогу

**Модуль:** catalog  
**Статус:** перевірено кодом  
**Статус контракту:** реалізовано

## Мета

Надати API для завантаження, списку та видалення зображень, які згодом використовуються Product.

## Маршрути і методи

```http
GET    /api/catalog/media
POST   /api/catalog/media       Content-Type: multipart/form-data
DELETE /api/catalog/media/{id}
```

## Запит і відповідь

| Операція | Запит | Успішна відповідь | Помилки |
| --- | --- | --- | --- |
| List | без параметрів | `200` список ID, original file name, public URL, content type, storage key, status | — |
| Upload | поле form-data `file` | `200` з `mediaFileId`, `storageKey`, `publicUrl`, `contentType`, `originalFileName` | `400` validation/невідповідна сигнатура |
| Delete | route `id:int` | `204` | `400`, `404` |

Файл: JPEG/PNG/WebP, позитивний розмір, не більший за `CatalogMediaUpload:MaxFileSizeBytes`; signature має збігатися із заявленим content type.

## Безпека та побічні ефекти

- Поточний доступ: `AllowAnonymous`.
- Upload не ідемпотентний: кожен успішний повтор створює новий storage key.
- Delete має side effect у зовнішньому сховищі та БД.
