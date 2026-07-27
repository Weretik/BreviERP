# Технологічний стек

| Сфера | Поточний вибір | Правило для нової роботи |
| --- | --- | --- |
| Runtime | .NET 10 / C# | Дотримуйтеся nullable annotations і налаштованих analyzers. |
| HTTP | ASP.NET Core controllers | Тримайте контролери тонкими та використовуйте Mediator. |
| Mediator | Mediator 3.x | Використовуйте request/handler пари та наявний pipeline. |
| Mediator pipeline | RequestLogging, Performance, Validation, Exception, DomainEventDispatcher | Зберігайте порядок реєстрації; не розміщуйте бізнес-правила у behaviors. |
| Валідація | FluentValidation | Розміщуйте input validators поряд зі сценарієм. |
| Контракт результату | Ardalis.Result | Повертайте `Result` або `Result<T>`; використовуйте усталений API mapping. |
| Доступ до даних | EF Core 10 + Npgsql/PostgreSQL | Тримайте EF configuration та міграції в Infrastructure. |
| Репозиторії | Ardalis.Specification | Повторно використовуйте наявний стиль repository/specification, де застосовно. |
| Identity | ASP.NET Core Identity | Зберігайте conventions ролей/політик і fallback-auth. |
| Логування | Serilog | Лише структуровані, нечутливі логи. |
| Фонові задачі | наявні job abstractions | Не вводьте прямий scheduler coupling у Application. |
| Медіасховище каталогу | `AdmToolsMediaStorageService` | Тримайте зовнішній адаптер за Infrastructure-межою. |
| Тести | NUnit unit/integration/architecture проєкти | Додавайте тести пропорційно зміненій межі. |

Не вводьте пакет для заміни чинної project convention без явного рішення у специфікації функціональності та погодження.
