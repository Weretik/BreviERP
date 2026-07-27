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

## Підхід до startup і даних

`DbMigrator` застосовує EF migrations через provider execution strategy. `UseAppMigrations` запускає всі зареєстровані `IDatabaseMigrator`; `UseAppSeeders` запускає кожен унікальний зареєстрований `ISeeder` у scoped startup operation.

Ці startup extensions — host-level operations. Feature handler не може самостійно застосовувати migrations або seed data.

## Поведінка безпеки

Поточний користувач і permissions мають споживатися через application-facing abstractions. Не обходьте їх, читаючи `HttpContext` усередині Application.
