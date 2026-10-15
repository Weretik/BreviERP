# BuildingBlocks.Infrastructure

## Структура

```text
BuildingBlocks.Infrastructure/
├── DependencyInjection/
│   └── AddInfrastructureServices
├── DomainEvents/
│   ├── EfDomainEventContext
│   └── MediatorDomainEventDispatcher
├── Migrations/
│   ├── IDatabaseMigrator
│   └── DbMigrator
├── Seeding/
│   └── ISeeder
├── Extensions/
│   ├── UseAppMigrations
│   └── UseAppSeeders
└── Services/
    ├── FakeCurrentUserService
    └── FakePermissionService
```

## Зареєстровані спільні сервіси

`AddInfrastructureServices` реєструє:

- `IDomainEventContext` як `EfDomainEventContext`;
- `IDomainEventDispatcher` як `MediatorDomainEventDispatcher`;
- спільні технічні сервіси, передбачені поточною композицією хоста.

## Підхід до запуску й даних

`DbMigrator` застосовує міграції EF через стратегію виконання провайдера. `UseAppMigrations` запускає всі зареєстровані `IDatabaseMigrator`; `UseAppSeeders` запускає кожен унікальний зареєстрований `ISeeder` в операції запуску з окремою областю залежностей.

Ці розширення запуску виконуються на рівні хоста. Обробник фічі не може самостійно застосовувати міграції або дані початкового наповнення.

## Поведінка безпеки

Поточний користувач і дозволи мають споживатися через абстракції Application. Не обходьте їх, читаючи `HttpContext` усередині Application.
