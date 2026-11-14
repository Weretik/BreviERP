# Domain Structure

## Responsibility

Domain owns the business model and its invariants. It must be understandable without knowledge of ASP.NET Core, EF Core, SQL, HTTP, or external clients.

## Target module structure

Folders follow aggregates, not kinds of classes. There are no `Entities/`, `Enums/`, `Errors/`, `ValueObjects/`, or `Constants/` folders.

```text
<Module>.Domain/
├── <Aggregate>/              root, child entities, value objects, enums, errors, events of one aggregate
│   └── <Part>/               only when the aggregate grows: a sub-concept such as Lines/, never a type
├── <SharedConcept>/          a value object used by several aggregates, named after the concept (Money/, Slugs/)
└── <Module>.Domain.csproj
```

Example: `Production.Domain/Orders/` holds `ProductionOrder.cs`, `ProductionOrderItem.cs`, `ProductionOrderStatus.cs`, and their errors. The shared rule text is in the [module rules](../../standards/module-rules.md#folder-structure-inside-a-module).

## Placement diagram

```text
Application command
        │ invokes behavior
        ▼
Aggregate / Entity ───→ Value object
        │ enforces invariant │ validates concept
        ├──→ Domain error
        └──→ Domain event (collected here, not dispatched here)
```

## Rules

- Factories and behavior methods enforce invariants.
- Use a value object when a primitive type would allow an invalid state.
- Domain events describe a completed business fact; an entity collects them.
- Request DTOs, DbSets, EF configuration, repository implementations, HTTP calls, and logging dependencies do not belong here.
