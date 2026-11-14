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
- [Module rules](../standards/module-rules.md) — module shape, Contracts, folders, tests, hosts and jobs

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

## Structure migration

The audit of 2026-11-14 found that the code does not yet follow the [module rules](../standards/module-rules.md). It moves to them in stages, one pull request each; the open items are the [known deviations](../standards/module-rules.md#known-deviations).

| Stage | Scope |
| --- | --- |
| 1 | Rules, layer documents, [ADR-0006](decisions/0006-module-contracts.md) |
| 2-3 | `<Module>.Contracts` projects; host adapters move into modules; `Add<Module>Module()`; container validation in every host |
| 3b | `Directory.Build.props` and `Directory.Packages.props` at the solution root |
| 3c | `BuildingBlocks` keeps only code every module needs |
| Outbox | [Transactional outbox](../standards/module-rules.md#transactional-outbox): one table, one protected dispatch endpoint, one Cloud Scheduler job |
| Jobs audit | Jobs projects, Cloud Run Jobs and Cloud Scheduler entries: a report with proposed merges before any cloud change |
| 4 | Domain folders by aggregate |
| 5 | Application `Features/<Feature>/{Commands,Queries}/<UseCase>/` |
| 6 | Infrastructure `Persistence/<Aggregate>/`, `Integrations/<System>/` |
| 7 | Api `Features/<Feature>/` |
| 8 | Jobs and `Host.Seed` |
| 9 | Test structure and architecture tests for every rule |
| 10 | Feature cards with code paths per layer, final audit and its fixes |
| Last | Controllers to Minimal API, endpoints grouped by feature (`Endpoints/<Feature>/`, one endpoint per use case with its request next to it), the OpenAPI contract unchanged |
