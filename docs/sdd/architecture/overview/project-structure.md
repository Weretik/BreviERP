# Project and Layer Structure

## Solution Map

```text
BreviERP.sln
├── src/
│   ├── Bootstrapper/
│   │   ├── Host.Api/                 composition root and HTTP host: Program.cs, DependencyInjection/, Security/
│   │   └── Host.Seed/                migrations, seed and Cloud Run jobs: picks a job and runs it
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Domain/
│   │   ├── BuildingBlocks.Application/
│   │   ├── BuildingBlocks.Infrastructure/
│   │   └── BuildingBlocks.Api/
│   ├── Jobs/{CatalogImport,SewingPricing}/   one-off data imports run by Host.Seed
│   └── Modules/{Accounting,Ai,Catalog,Content,Crm,Identity,Media,Production,Purchasing,Reference,Reminders,Warehouse}/
│       └── <Module>.{Api,Application,Contracts,Domain,Infrastructure}/
├── tests/
│   ├── UnitTests/<Module>/{Domain,Application}/
│   ├── IntegrationTests/<Module>/{Api,Persistence,Integrations,Contracts}/ and Platform/
│   ├── HostApiTests/
│   └── ArchitectureTests/<Rule>/
└── docs/sdd/                         active documentation
```

Folders inside each project are described in the [module rules](../../standards/module-rules.md#folder-structure-inside-a-module) and the layer documents: [Domain](../layers/domain-structure.md), [Application](../layers/application-structure.md), [Infrastructure](../layers/infrastructure-structure.md), [API and Host](../layers/api-host-structure.md).

## Dependency Diagram

```text
                    Host.Api / Host.Seed
                            │ Add<Module>Module()
                            ▼
                      <Module>.Api ──────────────┐ (DependencyInjection only)
                            │                    ▼
                            │          <Module>.Infrastructure ──┐
                            ▼                    │               │
                   <Module>.Application ◄────────┘               │
                     │            │                              │
                     ▼            ▼                              ▼
             <Module>.Domain   <Module>.Contracts ◄──── <Other>.Contracts
                                  (no module references)   (the only part of
                                                            another module a
                                                            module may use)
```

Application, Infrastructure, and Api of a module may reference other modules' `Contracts` projects and nothing else from them ([ADR-0006](../decisions/0006-module-contracts.md)).
