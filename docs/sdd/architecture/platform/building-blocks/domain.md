# BuildingBlocks.Domain

## Structure

```text
BuildingBlocks.Domain/
├── Abstractions/
│   ├── IEntity, IEntityId
│   ├── IAggregateRoot
│   ├── IDomainEvent, IHasDomainEvents
│   └── IDomainError
├── Entity/
│   ├── BaseEntity
│   └── BaseAuditableEntity
├── ValueObjects/
│   ├── MoneyAmount
│   └── CategoryPath
└── Exceptions/
    └── DomainException
```

## Available patterns

- `BaseEntity` provides identity and equality by concrete type and non-transient ID, as well as methods for collecting domain events.
- `BaseAuditableEntity` adds audit fields and explicit change-marking methods.
- `DomainException` carries `IDomainError`; `ExceptionBehavior` converts it to `Result.Invalid` for handlers that return Result.
- `IAggregateRoot` is a marker for aggregate roots; `IHasDomainEvents` is the event lifecycle contract.
- `MoneyAmount` and `CategoryPath` are existing shared value objects and should not be duplicated with the same semantics.

## Usage

Inherit an aggregate or entity from the corresponding base only when its ID and event lifecycle fit. Add an event through `AddDomainEvent` inside a completed domain operation. Do not publish it from Domain.

Do not add EF attributes, HTTP concepts, repository implementations, or transport DTOs here.
