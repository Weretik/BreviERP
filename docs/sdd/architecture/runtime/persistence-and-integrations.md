# Persistence and Integrations

## Persistence

Catalog, Reference, Identity, and Accounting use separate EF Core contexts on PostgreSQL. Context configuration, migrations, and entity configurations stay in the owning module's Infrastructure project.

- Use `AsNoTracking` for read-only queries.
- Use server-side projections and pagination; avoid N+1 queries and full materialization for paged endpoints.
- Repositories and Ardalis.Specification are used where the module already follows that style.
- There is no shared `IUnitOfWork` in the current code. If a use case needs coordinated writes across multiple aggregates or contexts, define the transaction boundary and partial-execution risk in the specification before implementation.
- Do not add migrations or change schema or concurrency semantics without an explicit feature requirement.

## External adapters

Catalog media storage and other external adapters live in Infrastructure. Application owns only the abstraction and use-case orchestration.

There is no shared `IBackgroundJobService` abstraction or implemented scheduler in the current code. For new background work, first define its contract and boundaries in the specification. Do not couple Application code directly to a scheduler.

## Read models

For read endpoints, use projection-oriented DTOs and no-tracking strategies where supported by the module. A read model is not an aggregate that owns business rules.
