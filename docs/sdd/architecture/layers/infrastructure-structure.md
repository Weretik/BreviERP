# Infrastructure Structure

## Responsibility

Infrastructure is the adapter layer. It implements Application contracts and stores/transmits data through technology-specific systems.

## Target module structure

```text
<Module>.Infrastructure/
├── DataBase/
│   └── <Module>DbContext.cs
├── Configurations/          EF IEntityTypeConfiguration classes
├── Migrations/              EF migrations and model snapshot
├── Repositories/            EF repository implementations
├── Projections/             building read models from persisted data
├── Integrations/            HTTP, queue, and storage adapters
├── Services/                storage, export, and notification adapters
├── DependencyInjection/     Add<Module>... extension methods
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
- Register adapters through the module's existing DI extension method.
- Infrastructure may reference Application and Domain; the reverse direction is forbidden.
- Keep external-data mapping at the adapter boundary and do not let generated client types leak inward.
