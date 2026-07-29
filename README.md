# BreviERP

Backend-система ERP для Brevi. Репозиторій містить модульний ASP.NET Core API, бізнес-модулі та інфраструктуру для роботи з каталогом, довідниками, користувачами й обліком.

> Проєкт перебуває в активній розробці. API-контракти та реалізовані можливості описані в [документації](docs/README.md).

## Технології

- .NET 10 та C#
- ASP.NET Core Web API
- PostgreSQL та Entity Framework Core
- Clean Architecture, DDD і CQRS
- Mediator, FluentValidation та Ardalis.Result
- ASP.NET Core Identity, Serilog і OpenAPI/Swagger
- NUnit для unit, integration та architecture тестів

## Модулі

| Модуль | Відповідальність |
| --- | --- |
| Identity | Користувачі, ролі, сесії та авторизація. |
| Catalog | Товари, категорії та медіафайли каталогу. |
| Reference | Постачальники, тканини, фурнітура, частини виробів та операції. |
| Accounting | Гаманці, транзакції та їхні категорії. |
| Crm | Виділена модульна межа для CRM; предметні API-сценарії ще в роботі. |

## Архітектура

Проєкт дотримується модульної Clean Architecture. Залежності спрямовані до домену:

```text
Host.Api / Host.Seed
        │
        ├── Module.Api ──→ Module.Application ──→ Module.Domain
        └── Module.Infrastructure ──────────────→ Module.Application + Module.Domain
```

`BuildingBlocks` містить спільні технічні та доменні примітиви й не залежить від бізнес-модулів.

## Швидкий старт

### Передумови

- .NET SDK 10
- PostgreSQL, доступний із локального середовища
- локальні секрети та рядок підключення до бази даних

Клонуйте репозиторій і виконайте:

```powershell
dotnet restore BreviERP.sln
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile https
```

API буде доступний за адресою `https://localhost:7142`, а health-check — за `https://localhost:7142/health`.

У середовищі `Development` Swagger UI доступний за адресою `https://localhost:7142/swagger`.

Для запуску без HTTPS:

```powershell
dotnet run --project src/Bootstrapper/Host.Api/Host.Api.csproj --launch-profile http
```

## Конфігурація

`Host.Api` використовує стандартну конфігурацію ASP.NET Core. Для чутливих локальних значень використовуйте User Secrets або змінні середовища, наприклад `ConnectionStrings__Default`.

```powershell
dotnet user-secrets set "ADMIN_DEFAULT_PASSWORD" "<локальний-пароль>" --project src/Bootstrapper/Host.Api/Host.Api.csproj
```

Не додавайте паролі, токени або production connection strings до репозиторію. Повний перелік налаштувань є в [документації з конфігурації](docs/sdd/operations/configuration/configuration-and-secrets.md).

## Міграції та початкові дані

Для контрольованого застосування міграцій і початкового наповнення використовується окремий хост:

```powershell
dotnet run --project src/Bootstrapper/Host.Seed/Host.Seed.csproj
```

Ця команда може змінити схему та дані бази. Перед запуском, особливо не на локальній БД, прочитайте [інструкцію про міграції та seeders](docs/sdd/operations/data/migrations-and-seeding.md).

## Перевірка

```powershell
dotnet restore BreviERP.sln
dotnet build BreviERP.sln --no-restore
dotnet test BreviERP.sln --no-build
```

## Структура репозиторію

```text
BreviERP.sln
├── src/
│   ├── Bootstrapper/       # HTTP API та хост початкового наповнення
│   ├── BuildingBlocks/     # спільні абстракції та кросфункціональний код
│   └── Modules/            # бізнес-модулі та їхні шари
├── tests/                  # unit, integration та architecture тести
└── docs/                   # versioned engineering documentation
```

## Документація

- [Архітектура](docs/sdd/architecture/README.md)
- [Інженерні правила](docs/sdd/standards/README.md)
- [Локальний запуск і експлуатація](docs/sdd/operations/README.md)
- [Специфікації функціональностей](docs/sdd/specs/README.md)
- [OpenAPI-контракт](docs/sdd/contracts/openapi.yaml)

## Ліцензія

Дивіться [LICENSE.txt](LICENSE.txt).
