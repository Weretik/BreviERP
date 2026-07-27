# Persistence та інтеграції

## Persistence

Catalog, Reference, Identity та Accounting використовують окремі EF Core contexts на PostgreSQL. Конфігурація context-ів, migrations і entity configurations залишаються в Infrastructure-проєкті власника.

- Використовуйте `AsNoTracking` для read-only queries.
- Використовуйте projections і server-side paging; уникайте N+1 queries та повної materialization для pageable endpoints.
- Repositories і Ardalis.Specification застосовуються там, де модуль уже використовує цей стиль.
- `IUnitOfWork` залишається абстракцією transaction boundary для скоординованих writes.
- Не додавайте міграцію, не змінюйте schema та concurrency semantics без явної вимоги функціональності.

## Зовнішні адаптери

Медіасховище Catalog та інші зовнішні adapters розміщуються в Infrastructure. Application володіє лише абстракцією та use-case orchestration.

Фонова робота має використовувати наявну `IBackgroundJobService` abstraction або зареєстрований job pattern. Не зв’язуйте Application code напряму зі scheduler-ом.

## Read models

Для read endpoints використовуйте projection-focused DTO та no-tracking strategy, коли це підтримано модулем. Read model не є aggregate-ом, який володіє бізнес-правилами.
