# ADR-0019: The module graph has no cycle

**Status:** accepted · **Date:** 2026-11-18 · **Owner:** Виталий (project owner) · **Extends:** [ADR-0006](0006-module-contracts.md)

## Context

[ADR-0006](0006-module-contracts.md) lets a module use another module only through its `Contracts` project. It did not say which way a reference may point. By 2026-11-17 five pairs of modules referenced each other's `Contracts` both ways, and the DDD audit listed them as a known deviation:

| Pair | Downstream needs of upstream | Upstream asked of downstream |
| --- | --- | --- |
| Crm ↔ Production | Production makes orders from КП (`IQuoteReader`, `IEmbroideryCalculationReader`) | Crm asked which КП have an order (`IQuoteOrderLinks` in Production.Contracts) |
| Production ↔ Warehouse | Warehouse plans and issues fabric for orders (`IOrderFabricPlans`) | «Звіти цеху» asked for issues and balances (`IStockFacts` in Warehouse.Contracts) |
| Catalog ↔ Reference | Catalog prices from fabrics, accessories, operations and parameters | Reference asked whether a product holds a row (`ICatalogUsage`), told Catalog that prices changed (`PricingInputChanges`) and that parameters changed (`IPricingSettingsSync`) |
| Catalog ↔ Media | Catalog shows library photos | Media asked which products use a photo (`ICatalogUsage`) |
| Crm ↔ Media | Crm stores КП photos and embroidery «Макет» | Media asked which calculations use a photo (`IEmbroideryCalculationReader.MockupUsagesAsync`) |

The architecture test written for this change also found a longer cycle: ActivityLog → Identity (staff names) → Media (avatars) → ActivityLog (every module writes «Журнал дій»).

With a cycle, the two modules can no longer change or be tested apart, a contract of one names the other's concepts (`ICatalogUsage` held Reference and Media questions), and a later split into services would need both at once.

## Options

1. **Keep the mutual references.** Rejected: the owner's rule is to fix architecture violations, not to keep them.
2. **Turn every upstream question into an outbox message.** Rejected for these cases: «is this photo used?» and «is this fabric used?» are answered before a delete, in the same request; a message arrives too late. The outbox is kept where a reaction must survive a failure.
3. **Move the shared part to a third module.** Rejected: no shared concept exists; each question belongs to the upstream module's own feature.
4. **The upstream module declares a port in its own `Contracts`; the downstream module implements it (chosen).** Dependency inversion across modules: the upstream module asks every registered implementation (`IEnumerable<T>`) and knows none of them.

## Decision

The module graph (module A → module B when a project of A references `B.Contracts`) has no cycle. When an upstream module needs something of a downstream module, the upstream module declares a small port in its own `Contracts`, named in its own words, and calls every registered implementation; the downstream module implements it in `Infrastructure/Adapters/<Upstream>/` and registers it in its own `Add<Module>Module()`. A reaction that must survive a failure is told from the upstream module's outbox.

| Pair | Port (declared by) | Implemented by |
| --- | --- | --- |
| Crm ← Production | `IQuoteOrderLinks` (Crm.Contracts/Quotes) | Production `Adapters/Crm/QuoteOrderLinks` |
| Production ← Warehouse | `IStockFacts` (Production.Contracts/FabricPlans) | Warehouse `StockFacts` |
| Reference ← Catalog | `IPricingInputUsageSource`, `IPricingInputSubscriber`, `IPricingSettingsSubscriber` (Reference.Contracts/Pricing); the last one from the Reference outbox | Catalog `Adapters/Reference/` |
| Media ← Catalog, Crm | `IMediaFileUsageSource` (Media.Contracts/Files) | Catalog `Adapters/Media/CatalogMediaUsage`, Crm `Adapters/Media/EmbroideryMediaUsage` |
| ActivityLog ← Identity | `IActivityActorNames` (ActivityLog.Contracts/Actors) | Identity `Adapters/ActivityLog/StaffActivityActorNames` |

`PricingInputChanges` is no longer a contract: only Catalog uses it, so it moved to `Catalog.Application`. `ICatalogUsage` and `IPricingSettingsSync` are gone; `Warehouse.Contracts` offers nothing for now.

The rule text is in the [module rules](../../standards/module-rules.md#communication-between-modules).

## Consequences

- Modules: references now point downstream only (see the [C4 component diagram](../c4/README.md#level-3-components-of-the-backend) and the [module map](../overview/modules.md)). ActivityLog references no module.
- Behaviour, HTTP API and data: unchanged; no schema change. The outbox message type `reference.pricing-settings.changed` keeps its name, so messages saved before the deploy are still handled.
- Tests: `ContractsDependencyTests.Modules_do_not_depend_on_each_other_in_a_cycle` builds the graph from the `.csproj` files and fails on any cycle; `ActivityLogDependencyTests` checks that ActivityLog references no module.
- A downstream module that is not registered (a host or test without it) simply adds no implementation; the upstream module then sees no usage and no subscriber.
