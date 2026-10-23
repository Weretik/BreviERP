# Project and Layer Structure

## Solution Map

```text
BreviERP.sln
├── src/
│   ├── Bootstrapper/
│   │   ├── Host.Api/                 composition root and HTTP host
│   │   └── Host.Seed/                seeding host
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Domain/
│   │   ├── BuildingBlocks.Application/
│   │   ├── BuildingBlocks.Infrastructure/
│   │   └── BuildingBlocks.Api/
│   └── Modules/{Identity,Catalog,Reference,Accounting,Crm}/
│       └── <Module>.{Api,Application,Domain,Infrastructure}/
├── tests/{UnitTests,IntegrationTests,ArchitectureTests}/
└── docs/sdd/                         active documentation
```

## Dependency Diagram

```text
                         Host.Api / Host.Seed
                          │        │
             ┌────────────┘        └────────────┐
             ▼                                  ▼
        <Module>.Api                    <Module>.Infrastructure
             │                                  │
             └────────────────┐     ┌──────────┘
                              ▼     ▼
                      <Module>.Application
                              │
                              ▼
                       <Module>.Domain
```

Not every module must use every project, but it must preserve the direction shown.
