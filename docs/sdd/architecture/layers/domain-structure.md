# Domain Structure

## Responsibility

Domain owns the business model and its invariants. It must be understandable without knowledge of ASP.NET Core, EF Core, SQL, HTTP, or external clients.

## Target module structure

```text
<Module>.Domain/
├── Entities/                 aggregates and entities
│   └── <Area>/
├── ValueObjects/             validated conceptual values and typed IDs
├── Errors/                   domain error factories/codes
├── Events/                   domain events
├── Services/                 pure domain services only
├── Specifications/           domain-level specifications, where applicable
├── Constants/
└── <Module>.Domain.csproj
```

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
