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

- `BaseEntity` надає ідентичність і порівняння за конкретним типом та неперехідним ID, а також методи збирання доменних подій.
- `BaseAuditableEntity` додає поля аудиту й явні методи позначення змін.
- `DomainException` переносить `IDomainError`; `ExceptionBehavior` перетворює його на `Result.Invalid` для обробників, що повертають Result.
- `IAggregateRoot` є маркером коренів агрегатів; `IHasDomainEvents` — контрактом життєвого циклу подій.
- `MoneyAmount` і `CategoryPath` — наявні спільні об'єкти-значення, які не слід дублювати з тією ж семантикою.

## Використання

Наслідуйте агрегат або сутність від відповідної бази лише коли її ID та життєвий цикл подій підходять. Додавайте подію через `AddDomainEvent` усередині завершеної доменної операції. Не публікуйте її з Domain.

Не додавайте сюди атрибути EF, поняття HTTP, реалізації репозиторіїв або транспортні DTO.
