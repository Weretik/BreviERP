# Прямі NuGet-залежності BreviERP

**Перевірено:** 2026-10-15 за файлами `.csproj` у `src/` і `tests/`. Нижче наведено всі 61 пряме оголошення `PackageReference` (45 унікальних пар назва/версія), а не транзитивні залежності. Версії в `.csproj` є джерелом істини; після оновлення пакета змінюйте цей каталог у тому самому коміті. Проєкт-власник вказаний заголовком таблиці. Призначення описує роль пакета, але саме оголошення залежності не доводить її використання під час виконання. Позначка «підтверджено в коді» стосується використання в репозиторії, а не обов'язково в кожному проєкті-власнику.

## Роль груп залежностей

- **Host.Api:** ASP.NET Core, генератор вихідного коду Mediator, OpenAPI/Swagger, Serilog, інструменти EF Core та шаблонізатор Scriban. Конкретну поведінку перевіряйте в `src/Bootstrapper/Host.Api/Program.cs` і реєстраціях залежностей.
- **Host.Seed:** окремий хост міграцій і початкового наповнення; його запуск описано в [операційній інструкції](../../operations/data/migrations-and-seeding.md).
- **BuildingBlocks.Domain:** типізовані значення й доменні примітиви; **BuildingBlocks.Application:** результати, специфікації, валідація, Mediator, EF/Npgsql та спільні сервіси; **BuildingBlocks.Api:** відображення результатів у HTTP.
- **Catalog.Infrastructure:** нормалізація slug і транслітерація для адаптерів модуля.
- **Тести:** NUnit, SDK й адаптер тестування, імітатори, покриття, API TestHost та перевірка YAML/OpenAPI. Пакети тестів не слід видавати за залежності під час виконання.

## Приклади перевіреного використання

| Підхід | Джерело в коді |
| --- | --- |
| Mediator і порядок поведінок | [реєстрація конвеєра](../../../../src/Bootstrapper/Host.Api/DependencyInjection/ServiceRegistration/Pipeline/MediatorRegistrationExtensions.cs) |
| FluentValidation | [реєстрація валідаторів](../../../../src/Bootstrapper/Host.Api/DependencyInjection/ServiceRegistration/Pipeline/FluentValidationRegistrationExtensions.cs) |
| EF Core з PostgreSQL | [реєстрація Catalog DbContext](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/DependencyInjection/CatalogDbContextExtensions.cs) |
| Serilog, Swagger і OpenAPI | [HTTP-хост](../../../../src/Bootstrapper/Host.Api/Program.cs) |
| Slugify.Core і TranslitKit | [генератор slug Catalog](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Products/SlugGeneration/ProductSlugGenerator.cs) |
| libphonenumber-csharp | [допоміжний сервіс телефонних номерів](../../../../src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/Helpers/PhoneNumberHelper.cs) |
| Moq і NUnit | [модульні тести Product](../../../../tests/UnitTests/ProductCommandHandlerTests.cs) |
| YamlDotNet | [перевірка OpenAPI-контракту сесії](../../../../tests/UnitTests/SessionOpenApiContractTests.cs) |

Для пакета зі статусом «оголошено» перед твердженням про його фактичне використання знайдіть виклик, тип або конфігурацію в коді. Якщо пакет виявиться непотрібним, його видалення потребує окремої перевірки збірки й тестів.

## [Host.Api.csproj](../../../../src/Bootstrapper/Host.Api/Host.Api.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `Mediator.SourceGenerator` | `3.0.1` | Генерація реалізації Mediator | Використання підтверджено в коді |
| `Microsoft.AspNetCore.OpenApi` | `10.0.2` | Генерація OpenAPI у хості | Використання підтверджено в коді |
| `Microsoft.EntityFrameworkCore.Tools` | `10.0.2` | Інструменти команд EF Core | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.OpenApi` | `2.7.5` | Модель OpenAPI | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Scriban` | `7.2.3` | Шаблонізатор тексту | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog.AspNetCore` | `10.0.0` | Інтеграція Serilog з ASP.NET Core | Використання підтверджено в коді |
| `Serilog.Enrichers.Environment` | `3.0.1` | Поля середовища для журналів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog.Enrichers.Process` | `3.0.0` | Поля процесу для логів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog.Settings.Configuration` | `10.0.0` | Налаштування Serilog з конфігурації | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog` | `4.3.0` | Структуроване логування | Використання підтверджено в коді |
| `Swashbuckle.AspNetCore` | `10.0.0` | Інструменти Swagger UI та OpenAPI | Використання підтверджено в коді |

## [Host.Seed.csproj](../../../../src/Bootstrapper/Host.Seed/Host.Seed.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `Serilog.AspNetCore` | `10.0.0` | Інтеграція Serilog з ASP.NET Core | Використання підтверджено в коді |

## [BuildingBlocks.Api.csproj](../../../../src/BuildingBlocks/BuildingBlocks.Api/BuildingBlocks.Api.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `Ardalis.Result` | `10.1.0` | Стандартизовані результати сценаріїв та їх відображення в HTTP | Використання підтверджено в коді |

## [BuildingBlocks.Application.csproj](../../../../src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application/BuildingBlocks.Application.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `Ardalis.Result.FluentValidation` | `10.1.0` | Перетворення помилок FluentValidation у результати | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Ardalis.Result` | `10.1.0` | Стандартизовані результати сценаріїв та їх відображення в HTTP | Використання підтверджено в коді |
| `Ardalis.Specification.EntityFrameworkCore` | `9.3.1` | Виконання специфікацій у EF Core | Використання підтверджено в коді |
| `Ardalis.Specification` | `9.3.1` | Опис правил вибірки та специфікацій читання | Використання підтверджено в коді |
| `DotNetEnv` | `3.1.1` | Завантаження `.env`-конфігурації | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `FluentValidation.DependencyInjectionExtensions` | `12.1.1` | Реєстрація валідаторів у контейнері залежностей | Використання підтверджено в коді |
| `FluentValidation` | `12.1.1` | Валідація вхідних команд і запитів | Використання підтверджено в коді |
| `libphonenumber-csharp` | `9.0.23` | Нормалізація та перевірка телефонних номерів | Використання підтверджено в коді |
| `Mediator.Abstractions` | `3.0.1` | Контракти запитів та обробників Mediator | Використання підтверджено в коді |
| `Microsoft.AspNetCore.Components.Authorization` | `10.0.2` | Контракти авторизації ASP.NET Core | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.AspNetCore.Http.Abstractions` | `2.3.9` | Абстракції HTTP | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | `10.0.2` | Збереження даних Identity через EF Core | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.EntityFrameworkCore.Design` | `10.0.2` | Підтримка EF Core під час проєктування | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.EntityFrameworkCore.Relational` | `10.0.2` | Реляційні можливості EF Core | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.EntityFrameworkCore` | `10.0.2` | ORM і DbContext | Використання підтверджено в коді |
| `Microsoft.Extensions.Logging.Abstractions` | `10.0.2` | Абстракції журналювання | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | `10.0.0` | Провайдер EF Core для PostgreSQL | Використання підтверджено в коді |
| `Serilog.AspNetCore` | `10.0.0` | Інтеграція Serilog з ASP.NET Core | Використання підтверджено в коді |
| `Serilog.Enrichers.Environment` | `3.0.1` | Поля середовища для логів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog.Enrichers.Process` | `3.0.0` | Поля процесу для логів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog.Enrichers.Thread` | `4.0.0` | Поля потоку для логів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog.Settings.Configuration` | `10.0.0` | Налаштування Serilog з конфігурації | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog.Sinks.Console` | `6.1.1` | Виведення журналів у консоль | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Serilog.Sinks.Seq` | `9.0.0` | Виведення логів у Seq | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Unidecode.NET` | `2.2.1` | Транслітерація Unicode | Оголошено в `.csproj`; фактичне використання окремо не перевірено |

## [BuildingBlocks.Domain.csproj](../../../../src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `Ardalis.SmartEnum` | `8.2.0` | Типізовані доменні переліки | Використання підтверджено в коді |
| `Vogen` | `8.0.4` | Типізовані об'єкти-значення | Використання підтверджено в коді |

## [Catalog.Infrastructure.csproj](../../../../src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Catalog.Infrastructure.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `Slugify.Core` | `5.1.1` | Генерація короткої назви для URL | Використання підтверджено в коді |
| `TranslitKit` | `1.0.3` | Транслітерація назви перед генерацією короткої назви для URL | Використання підтверджено в коді |

## [ArchitectureTests.csproj](../../../../tests/ArchitectureTests/ArchitectureTests.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `Microsoft.NET.Test.Sdk` | `18.0.1` | Запуск .NET-тестів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `NUnit.Analyzers` | `4.11.2` | Аналізатори NUnit | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `NUnit` | `4.4.0` | Середовище тестування | Використання підтверджено в коді |
| `NUnit3TestAdapter` | `6.1.0` | Підключення NUnit до засобу запуску тестів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |

## [IntegrationTests.csproj](../../../../tests/IntegrationTests/IntegrationTests.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `coverlet.collector` | `6.0.4` | Збір покриття тестів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.AspNetCore.TestHost` | `10.0.2` | Тестовий HTTP-хост | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.NET.Test.Sdk` | `18.0.1` | Запуск .NET-тестів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Moq` | `4.20.72` | Тестові об'єкти-імітації | Використання підтверджено в коді |
| `NUnit.Analyzers` | `4.11.2` | Аналізатори NUnit | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `NUnit` | `4.4.0` | Середовище тестування | Використання підтверджено в коді |
| `NUnit3TestAdapter` | `6.1.0` | Підключення NUnit до засобу запуску тестів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |

## [UnitTests.csproj](../../../../tests/UnitTests/UnitTests.csproj)

| Пакет | Версія з `.csproj` | Призначення пакета | Статус перевірки |
| --- | --- | --- | --- |
| `coverlet.collector` | `6.0.4` | Збір покриття тестів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.NET.Test.Sdk` | `18.0.1` | Запуск .NET-тестів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Microsoft.OpenApi.YamlReader` | `2.7.5` | Читання OpenAPI YAML у тестах | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `Moq` | `4.20.72` | Тестові об'єкти-імітації | Використання підтверджено в коді |
| `NUnit.Analyzers` | `4.11.2` | Аналізатори NUnit | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `NUnit` | `4.4.0` | Середовище тестування | Використання підтверджено в коді |
| `NUnit3TestAdapter` | `6.1.0` | Підключення NUnit до засобу запуску тестів | Оголошено в `.csproj`; фактичне використання окремо не перевірено |
| `YamlDotNet` | `16.3.0` | Парсинг YAML у тестах | Використання підтверджено в коді |

Після додавання нового пакета зафіксуйте в специфікації функціональності, яку потребу він вирішує, чому наявних засобів недостатньо, як він впливає на архітектуру, безпеку, розгортання й перевірку. [Технологічний стек](technology-stack.md) описує чинні підходи без дублювання цього переліку.
