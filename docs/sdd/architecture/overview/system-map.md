# System map

## Executable hosts

- `src/Bootstrapper/Host.Api` composes DI, middleware, controllers, authentication, and OpenAPI.
- `src/Bootstrapper/Host.Seed` is a console host for approved seed operations.
- `src/Jobs/CatalogImport` and `src/Jobs/SewingPricing` are operational job libraries (catalogue import, pricing verification and activation). Only `Host.Seed` references and runs them; Domain, Application, and `Host.Api` do not. See the [jobs overview](../../../../src/Jobs/README.md).

## Business modules

- Catalog owns products, categories, catalog media, Sewing Articles and composition, and Sewing/PPE pricing.
- Reference owns supplier, fabric, accessory, garment-part, and operation reference data.
- Identity owns ASP.NET Identity, roles, policies, current user/session integration, and seed data.
- Accounting owns wallets, transactions, and transaction categories.
- Crm is present as a module boundary but has no implemented domain endpoints.
- BuildingBlocks owns shared abstractions and cross-cutting implementations; it must not acquire business rules.

## Dependency direction

```text
Host.Api / Host.Seed ──→ Jobs.* (Host.Seed only)
        │
        ├── Module.Api ──→ Module.Application ──→ Module.Domain
        └── Module.Infrastructure ──────────────→ Module.Application + Module.Domain

BuildingBlocks.* supplies shared primitives; it does not depend on business modules.
```

Domain does not depend directly on Application or Infrastructure, and Application does not depend on a concrete DbContext, HTTP client, scheduler, or EF repository.
