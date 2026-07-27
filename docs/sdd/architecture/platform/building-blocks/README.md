# Довідка BuildingBlocks

BuildingBlocks — спільна технічна основа. Він визначає cross-module primitives і реалізації, але ніколи не володіє бізнес-правилами Catalog, Reference, Identity, Accounting або Crm.

## Карта проєктів

```text
src/BuildingBlocks/
├── BuildingBlocks.Domain/            примітиви сутностей, аудиту, помилок і подій
├── BuildingBlocks.Application/       Mediator behaviors, контракти, notifications і logging
├── BuildingBlocks.Infrastructure/    спільні adapter implementations, migrations і seeders
└── BuildingBlocks.Api/               Ardalis.Result → MVC mapping
```

## Читайте в такому порядку

1. [Domain primitives](domain.md)
2. [Application primitives і behaviors](application.md)
3. [Infrastructure services і startup](infrastructure.md)
4. [API result mapping](api.md)
5. [Правила використання та доступність](usage-rules.md)

Не створюйте інший shared base type, behavior або service abstraction до перевірки цієї довідки.
