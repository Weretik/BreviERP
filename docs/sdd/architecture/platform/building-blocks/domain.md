# BuildingBlocks.Domain

## Structure

```text
BuildingBlocks.Domain/
├── Abstractions/
│   ├── IEntity, IEntityId
│   ├── IAggregateRoot
│   └── IDomainEvent, IHasDomainEvents
├── Errors/
│   ├── Error
│   └── ErrorType
├── Results/
│   └── DomainResult, DomainResult<T>
├── Entity/
│   ├── BaseEntity
│   └── BaseAuditableEntity
└── ValueObjects/
    ├── MoneyAmount
    └── CategoryPath
```

## Available patterns

- `BaseEntity` provides identity and equality by concrete type and non-transient ID, as well as methods for collecting domain events.
- `BaseAuditableEntity` adds audit fields and explicit change-marking methods.
- A broken business rule is a returned `Error` (code with the module prefix, Ukrainian message, `ErrorType`): factories return `DomainResult<T>`, changing methods return `DomainResult`, and every check runs before the first change. Handlers turn it into an `Ardalis.Result` with `ErrorResults.ToResult()`. There is no domain exception; a `throw` is left only for a programmer error (a default id, a wrong argument from the server's own code) and becomes 500.
- `IAggregateRoot` is a marker for aggregate roots; `IHasDomainEvents` is the event lifecycle contract.
- `MoneyAmount` and `CategoryPath` are existing shared value objects and should not be duplicated with the same semantics.

## Usage

Inherit an aggregate or entity from the corresponding base only when its ID and event lifecycle fit. Add an event through `AddDomainEvent` inside a completed domain operation. Do not publish it from Domain.

Do not add EF attributes, HTTP concepts, repository implementations, or transport DTOs here.
