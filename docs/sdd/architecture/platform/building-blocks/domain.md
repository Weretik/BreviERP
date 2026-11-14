# BuildingBlocks.Domain

## Structure

```text
BuildingBlocks.Domain/
├── Abstractions/
│   ├── IEntity, IEntityId
│   └── IAggregateRoot
├── Errors/
│   ├── Error
│   └── ErrorType
├── Results/
│   └── DomainResult, DomainResult<T>
├── Entity/
│   ├── BaseEntity
│   └── BaseAuditableEntity
└── ValueObjects/
    └── MoneyAmount
```

## Available patterns

- `BaseEntity` provides identity and equality by concrete type and non-transient ID.
- `BaseAuditableEntity` adds audit fields and explicit change-marking methods.
- A broken business rule is a returned `Error` (code with the module prefix, Ukrainian message, `ErrorType`): factories return `DomainResult<T>`, changing methods return `DomainResult`, and every check runs before the first change. Handlers turn it into an `Ardalis.Result` with `ErrorResults.ToResult()`. There is no domain exception; a `throw` is left only for a programmer error (a default id, a wrong argument from the server's own code) and becomes 500.
- `IAggregateRoot` is a marker for aggregate roots. There are no domain events: no module raised one, so their dispatch was removed in stage 3c; a change another module or the outside world must learn about goes through the transactional outbox.
- `MoneyAmount` is the shared value object for money and should not be duplicated with the same semantics.

## Usage

Inherit an aggregate or entity from the corresponding base only when its ID fits.

Do not add EF attributes, HTTP concepts, repository implementations, or transport DTOs here.
