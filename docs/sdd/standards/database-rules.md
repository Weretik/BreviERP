# Database rules

## Читання та продуктивність

Для read use-cases використовуйте projections і `AsNoTracking`, де це доречно. Уникайте N+1 queries, unbounded list endpoints і непотрібної materialization. Pagination/filtering для великих списків має бути частиною контракту.

## Узгодженість і транзакції

Поважайте наявні transaction і concurrency boundaries. Не додавайте automatic retries до writes із побічними ефектами без задокументованої ідемпотентності. Не послаблюйте consistency guarantees непомітно.

## EF Core, міграції та дані

EF Core mapping належить Infrastructure. Domain entities не містять persistence concerns. Створюйте міграцію лише коли вона потрібна в scope feature; не редагуйте застосовані міграції. Перевіряйте нову міграцію на порожній test database та документуйте rollback/rollout ризики.

Зберігайте query semantics і database contracts. Довідникові/seed-дані змінюйте через наявний механізм seeding, а не прихованими runtime writes.
