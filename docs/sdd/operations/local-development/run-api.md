# Локальний запуск Host.Api

## Передумови

- .NET SDK, що відповідає target solution (`net10.0`).
- PostgreSQL, досяжний через `ConnectionStrings:Default`.
- Налаштовані потрібні локальні секрети; див. [конфігурацію та секрети](../configuration/configuration-and-secrets.md).

## Запуск

З кореня репозиторію:

```powershell
dotnet restore BreviERP.sln
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile https
```

Профіль `https` слухає `https://localhost:7142` і `http://localhost:5231`. Використовуйте HTTPS-адресу для перевірки автентифікації/refresh: refresh cookie налаштований для безпечного використання.

Для HTTP-only перевірки endpoint-а використовуйте профіль `http`:

```powershell
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile http
```

Перед запуском операцій, які застосовують міграції або seed-дані до не-локальної БД, прочитайте [міграції та seeders](../data/migrations-and-seeding.md).

## Зупинка та повторна збірка

Зупиніть запущений процес Host.Api перед build, якщо Windows заблокував його output DLL. Потім виконайте потрібні build/test-команди зі [стандартів тестування](../../standards/testing-rules.md).
