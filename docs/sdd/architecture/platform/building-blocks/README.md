# BuildingBlocks Reference

BuildingBlocks is the shared technical foundation. It defines primitives and implementations for multiple modules, but never owns the business rules of Catalog, Reference, Identity, Accounting, or Crm.

## Project Map

```text
src/BuildingBlocks/
├── BuildingBlocks.Domain/            entity, audit, error, and event primitives
├── BuildingBlocks.Application/       Mediator behaviors, contracts, notifications, and logging
├── BuildingBlocks.Infrastructure/    shared adapter implementations, migrations, and seeding
└── BuildingBlocks.Api/               Ardalis.Result → MVC mapping
```

## Read in this order

1. [Domain primitives](domain.md)
2. [Application primitives and behaviors](application.md)
3. [Infrastructure services and startup](infrastructure.md)
4. [API result mapping](api.md)
5. [Usage rules and availability](usage-rules.md)

Do not create another shared base type, pipeline behavior, or service abstraction before checking this reference.
