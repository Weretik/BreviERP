# Application Structure

## Responsibility

Application executes one use case at a time. It coordinates Domain and abstractions; it does not own persistence or transport implementations.

## Target feature structure

```text
<Module>.Application/
├── Features/
│   └── <Area>/<UseCase>/
│       ├── <UseCase>Command.cs | <UseCase>Query.cs
│       ├── <UseCase>CommandHandler.cs | <UseCase>QueryHandler.cs
│       ├── DTOs/               use-case-private DTOs, when needed
│       ├── Validators/
│       │   └── <UseCase>Validator.cs
│       ├── Specifications/     EF/Ardalis specifications for this use case only
│       └── Extensions/         cohesive LINQ/mapping helpers only
├── Contracts/
│   ├── Persistence/            DbContext/read/repository abstractions
│   ├── Integrations/           external service abstractions
│   └── Services/
├── Integrations/               application orchestration of external flows
├── Jobs/                       job use-case logic, never scheduler wiring
└── <Module>ApplicationAssemblyMarker.cs
```

## Use-case flow

```text
Command or Query
       │
       ▼
FluentValidation validator ── invalid ──→ Result.Invalid
       │ valid
       ▼
Handler ──→ Domain aggregate / Application abstraction
       │
       └──→ Result.Success | Result.NotFound | Result.Conflict | Result.Invalid
```

## Rules

- Commands change state; queries only read.
- A handler implements one use case and receives abstractions only.
- A query returning a list/detail read model without domain behavior uses `Specification<TEntity, TResult>` with `AsNoTracking` and a SQL `Select` into a DTO; the handler does not materialize an aggregate for manual mapping afterward.
- A paged list query returns `PagedResult<IReadOnlyList<TItem>>` with `PagedInfo`, not a local page wrapper. Use `Result.Map` for a pure mapping from one Result to another; use `Bind`/`BindAsync` for a sequence of Result-returning steps when that does not hide state checks.
- Validators handle input validation; Domain owns business invariants.
- Each specification belongs to one use case and lives in its `Specifications/`. Do not create a shared `<Area>/Specifications/` or import a specification from another use case. If two use cases share the same simple condition, each gets its own local specification; the only exception is truly shared code in an explicitly named `Shared/`, after the need is confirmed.
- Put public cross-module DTOs in the module contract, not next to a handler.
