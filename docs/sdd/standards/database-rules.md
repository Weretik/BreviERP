# Database rules

## Ідентифікатори

Правила вибору scalar type, typed Domain ID, `PublicId` та людського номера документа визначено у [стратегії ідентифікаторів](identifier-strategy.md). Зафіксуйте вибір і обґрунтування в `data-model.md` feature-специфікації до створення коду або migration.

## Читання та продуктивність

Для read use-cases використовуйте projections і `AsNoTracking`, де це доречно. Уникайте N+1 queries, unbounded list endpoints і непотрібної materialization. Pagination/filtering для великих списків має бути частиною контракту.

Для списку або detail read-model, що не потребує Domain behavior, базовий підхід —
`Ardalis.Specification.Specification<TEntity, TResult>` із SQL `Select` у response
DTO. Handler має отримати `TResult` з read repository, а не materialize `TEntity` і
потім мапити його в пам'яті. Додавайте лише потрібні `Include`/joins; projection з
navigations має повертати тільки використані поля. Перевіряйте integration-тестом,
що filters, sorting, pagination і projection транслюються у SQL.

## Узгодженість і транзакції

Поважайте наявні transaction і concurrency boundaries. Не додавайте automatic retries до writes із побічними ефектами без задокументованої ідемпотентності. Не послаблюйте consistency guarantees непомітно.

## EF Core, міграції та дані

EF Core mapping належить Infrastructure. Domain entities не містять persistence concerns. Створюйте міграцію лише коли вона потрібна в scope feature; не редагуйте застосовані міграції. Перевіряйте нову міграцію на порожній test database та документуйте rollback/rollout ризики.

Зберігайте query semantics і database contracts. Довідникові/seed-дані змінюйте через наявний механізм seeding, а не прихованими runtime writes.
