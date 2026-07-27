# BuildingBlocks.Domain

## Структура

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

## Доступні підходи

- `BaseEntity` надає identity, equality за concrete type та non-transient ID, а також methods collection доменних подій.
- `BaseAuditableEntity` додає audit fields і явні mark methods.
- `DomainException` переносить `IDomainError`; `ExceptionBehavior` мапить його до `Result.Invalid` для Result-based handlers.
- `IAggregateRoot` є marker для aggregate roots; `IHasDomainEvents` — контракт event lifecycle.
- `MoneyAmount` і `CategoryPath` — наявні спільні value objects, які не слід дублювати з тією ж семантикою.

## Використання

Наслідуйте aggregate/entity від відповідної бази лише коли її ID та event lifecycle підходять. Підіймайте event через `AddDomainEvent` усередині завершеної domain operation. Не dispatch-те його в Domain.

Не додавайте сюди EF attributes, HTTP concepts, repository implementations або transport DTO.
