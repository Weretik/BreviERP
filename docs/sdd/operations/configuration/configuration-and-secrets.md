# Конфігурація та секрети

## Джерела

`Host.Api` використовує стандартний стек конфігурації ASP.NET Core. У Development проєкт має User Secrets id; для локальних чутливих значень використовуйте user secrets. Конфігурація розгортання має передавати чутливі значення через погоджений механізм секретів/змінних середовища.

Ключі змінних середовища використовують подвійне підкреслення, наприклад `ConnectionStrings__Default` для `ConnectionStrings:Default`.

## Важливі налаштування

| Налаштування | Призначення | Секрет |
| --- | --- | --- |
| `ConnectionStrings:Default` | PostgreSQL-підключення для Identity та модульних контекстів | так |
| `ADMIN_DEFAULT_PASSWORD` | Пароль, який використовується лише якщо потрібно створити bootstrap-адміністратора | так |
| `Cors:AllowedOrigins` / `AllowedOriginsCsv` | Browser origins, яким дозволено викликати API | ні, але залежить від розгортання |
| `Identity:SessionCookies` | Імена cookie, шляхи, lifetime, правила `Secure` і SameSite | ні |
| `Identity:SessionSecurity` | Обмеження lifetime access/refresh token | ні |
| `AdmToolsStorage` | Параметри медіасховища Catalog | залежить від конкретного провайдера |
| `CatalogMediaUpload` | Максимальний розмір upload і базовий шлях оригіналів Product media | ні |

## Catalog media upload

`CatalogMediaUpload` є обов'язковою секцією конфігурації. Значення не мають
fallback у коді: `MaxFileSizeBytes` і `BaseFolder` задаються в
`appsettings.json` або в конфігурації deployment. API застосовує один і той
самий resolved limit і до multipart body, і до upload command.

Перед стартом Host перевіряє, що `MaxFileSizeBytes` більший за нуль, а
`BaseFolder` не порожній. Некоректна конфігурація блокує запуск, а не перший
upload-запит.

Production deployment може перевизначити значення без зміни файлу:

```text
CatalogMediaUpload__MaxFileSizeBytes=104857600
CatalogMediaUpload__BaseFolder=/products/original
```

Подвійне підкреслення в назві environment variable відповідає `:` у ключі
конфігурації .NET.

Приклад локального секрету:

```powershell
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<лише-локальне-значення>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
```

Ніколи не додавайте реальні паролі, токени чи production connection strings до `appsettings*.json`, Markdown, вихідного коду або логів. Не виводьте значення конфігурації під час діагностики проблеми запуску.

## Примітка щодо cookie та CORS

Refresh-автентифікація використовує cookie з credentials, тому CORS є явним allow-list; wildcard origins не використовуються. Перевіряйте refresh через HTTPS або створюйте свідоме локальне перевизначення, яке ніколи не переноситься до спільного середовища.
