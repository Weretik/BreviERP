# Backend rules

## Шари та залежності

Дотримуйтеся напрямку залежностей `API → Application → Domain`. Infrastructure залежить від внутрішніх шарів, але не навпаки.

- Domain володіє business invariants, entities, value objects і domain services; у ньому немає transport DTO або Infrastructure dependencies.
- Application оркеструє use-cases через abstractions, а не Infrastructure implementations.
- Infrastructure містить persistence та integration adapters; не володіє business rules.
- API виконує тільки HTTP binding, авторизацію та mapping результату; не містить EF або business logic.

## CQRS, Mediator і код

Використовуйте feature-by-folder усередині Application. Тримайте command/query, handler, validator і private DTO разом за сценарієм. Один Mediator handler володіє одним use-case; commands змінюють стан, queries не змінюють його.

Передавайте `CancellationToken` крізь увесь flow. Cross-cutting concerns залишайте у pipeline behaviors; не переміщуйте business rules у behaviors.

Використовуйте явні імена: `CreateOrderCommand`, `GetPublicProductListQuery`, `GetAdminProductListQueryHandler`. Не створюйте generic folders або names на кшталт `Common`, `Misc`, `Helper`, `Utils`, `Manager`. Не додавайте abstractions без конкретної потреби.

## Domain events і транзакції

Дотримуйтеся наявного lifecycle domain events: `collect → dispatch → clear`. Use-cases з кількома змінами aggregate-ів координують транзакцію через наявний `IUnitOfWork`.
