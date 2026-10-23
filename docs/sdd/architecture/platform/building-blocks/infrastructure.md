# BuildingBlocks.Infrastructure

## Structure

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

## Registered shared services

`AddInfrastructureServices` registers:

- `IDomainEventContext` as `EfDomainEventContext`;
- `IDomainEventDispatcher` as `MediatorDomainEventDispatcher`;
- shared technical services provided by the current host composition.

## Startup and data approach

`DbMigrator` applies EF migrations through the provider execution strategy. `UseAppMigrations` runs all registered `IDatabaseMigrator` instances; `UseAppSeeders` runs each unique registered `ISeeder` during startup in a separate dependency injection scope.

These startup extensions run at the host level. A feature handler must not apply migrations or seed data itself.

## Security behavior

The current user and permissions must be consumed through Application abstractions. Do not bypass them by reading `HttpContext` inside Application.
