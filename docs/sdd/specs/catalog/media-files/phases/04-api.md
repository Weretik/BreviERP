# Фаза 04: Публікація API

**Статус:** перевірено

`CatalogMediaController` зв’язує `multipart/form-data`, створює command із stream і конфігурованим max size, передає `CancellationToken` у `ISender` та мапить `Result`. Контролер не перевіряє format/signature — це відповідальність Application validator.

`DisableRequestSizeLimit` активний; фактичний ліміт контролює validator. Авторизація лишається `AllowAnonymous` за поточним рішенням.
