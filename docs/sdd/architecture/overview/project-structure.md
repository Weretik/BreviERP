# Структури проєкту та шарів

## Карта рішення

```text
BreviERP.sln
├── src/
│   ├── Bootstrapper/
│   │   ├── Host.Api/                 composition root і HTTP-хост
│   │   └── Host.Seed/                хост початкового наповнення
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks.Domain/
│   │   ├── BuildingBlocks.Application/
│   │   ├── BuildingBlocks.Infrastructure/
│   │   └── BuildingBlocks.Api/
│   └── Modules/{Identity,Catalog,Reference,Accounting,Crm}/
│       └── <Module>.{Api,Application,Domain,Infrastructure}/
├── tests/{UnitTests,IntegrationTests,ArchitectureTests}/
└── docs/sdd/                         активна документація
```

## Візуалізація залежностей

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

Не кожен модуль зобов’язаний використовувати кожен проєкт, але він має зберігати показаний напрямок.
