# Infrastructure Structure

## Responsibility

Infrastructure is the adapter layer. It implements Application contracts and stores/transmits data through technology-specific systems.

## Target module structure

Persistence is grouped by aggregate, with the same names as in Domain; integrations by external system. No flat `Configurations/`, `Repositories/`, `DataBase/`, or `Services/` folders.

```text
<Module>.Infrastructure/
├── Persistence/
│   ├── <Module>DbContext.cs, <Module>DbContextFactory.cs
│   ├── Migrations/               EF migrations and model snapshot
│   ├── Seeding/                  seed data of the module, when it has any
│   └── <Aggregate>/              entity configurations, repository, read queries of that aggregate
├── Integrations/<System>/        KeepinCrm, Dropbox, GoogleSheets, OpenAi, Telegram, Cdn
├── Contracts/<Feature>/          implementations of the module's own Contracts interfaces
├── Adapters/<Provider>/          adapters from another module's Contracts into this module's ports
├── DependencyInjection/          Add<Module>Infrastructure
└── <Module>.Infrastructure.csproj
```

## Adapter diagram

```text
Application abstraction
          ▲
          │ implemented by
Infrastructure adapter
     ├── EF Core DbContext ──→ PostgreSQL
     ├── external client ────→ storage / another service
     └── projection builder ─→ read model
```

## Rules

- EF configurations and migrations never belong in Domain or Application.
- Register adapters through the module's DI extension method; `Add<Module>Module()` in the Api project calls it.
- Moving a migration file between folders keeps its class name, namespace, and `[Migration]` id, so EF history does not change.
- Infrastructure may reference Application and Domain; the reverse direction is forbidden.
- Keep external-data mapping at the adapter boundary and do not let generated client types leak inward.
