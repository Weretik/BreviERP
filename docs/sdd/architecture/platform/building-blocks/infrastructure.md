# BuildingBlocks.Infrastructure

## Structure

```text
BuildingBlocks.Infrastructure/
├── Migrations/
│   ├── IDatabaseMigrator
│   └── DbMigrator
├── Seeding/
│   └── ISeeder
└── Extensions/
    ├── UseAppMigrations
    └── UseAppSeeders
```

## Startup and data approach

`DbMigrator` applies EF migrations through the provider execution strategy. `UseAppMigrations` runs all registered `IDatabaseMigrator` instances; `UseAppSeeders` runs each unique registered `ISeeder` during startup in a separate dependency injection scope.

These startup extensions run at the host level. A feature handler must not apply migrations or seed data itself.

## Security behavior

Application never reads `HttpContext`. The Api passes the signed-in user into the command (for example `By`), and access is checked by host policies before the handler runs.
