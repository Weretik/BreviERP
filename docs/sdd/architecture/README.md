# Backend architecture

BreviERP is a modular ASP.NET Core backend on .NET 10. It uses Clean Architecture, DDD, CQRS, Mediator, FluentValidation, Ardalis.Result, EF Core/PostgreSQL, and Serilog.

## Choose by question

### Need the big picture

- [System map](overview/system-map.md) — hosts, modules, and dependency direction.
- [Project structure](overview/project-structure.md) — solution tree and layer visualization.
- [Layer boundaries](overview/layers.md) — ownership and forbidden dependencies.
- [Module map](overview/modules.md) — Identity, Catalog, Reference, Accounting, Crm, and BuildingBlocks.
- [Database map](data-model/README.md) — tree of PostgreSQL schemas and tables with separate module descriptions.

### Need to place code in a layer

- [Domain](layers/domain-structure.md)
- [Application](layers/application-structure.md)
- [Infrastructure](layers/infrastructure-structure.md)
- [API and Host](layers/api-host-structure.md)

### Need to understand execution

- [Use-case flow](runtime/use-case-flow.md)
- [Mediator behaviors](runtime/mediator-behaviors.md)
- [Cross-cutting pipeline](runtime/cross-cutting.md)
- [Persistence and integrations](runtime/persistence-and-integrations.md)

### Need a platform reference

- [Technology stack](platform/technology-stack.md)
- [NuGet dependency roles](platform/nuget-packages.md)
- [BuildingBlocks reference](platform/building-blocks/README.md)

### Need the reason for a long-term decision

- [Architecture decisions](decisions/README.md)

### Need identity or access control

- [Identity, authentication, and authorization](security/README.md)

Architecture documents describe stable rules. A planned change belongs in `docs/sdd/specs`, not here, unless it changes a stable rule.
