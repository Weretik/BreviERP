# Application Structure

## Responsibility

Application executes one use case at a time. It coordinates Domain and abstractions; it does not own persistence or transport implementations.

## Target feature structure

Every use case is one folder under `Commands/` (changes state) or `Queries/` (reads only). Admin and public variants of one feature live in the same feature folder. The name `Contracts` is reserved for the module's `<Module>.Contracts` project.

```text
<Module>.Application/
├── Abstractions/                       ports of the module: DbContext/read abstractions, repositories, external services
├── Features/
│   └── <Feature>/
│       ├── Commands/<UseCase>/
│       │   ├── <UseCase>Command.cs
│       │   ├── <UseCase>Handler.cs
│       │   ├── <UseCase>Validator.cs
│       │   └── <UseCase>Specification.cs    EF/Ardalis specification of this use case only
│       ├── Queries/<UseCase>/
│       │   ├── <UseCase>Query.cs, <UseCase>Handler.cs, <UseCase>Validator.cs, <Name>Dto.cs
│       │   └── response DTOs of this query
│       ├── EventHandlers/                   reactions to domain events or other modules' integration events
│       └── Shared/                          DTOs, mapping, policies used by several use cases of this feature
├── DependencyInjection/
└── <Module>ApplicationAssemblyMarker.cs
```

No `DTOs/`, `Validators/`, `Specifications/`, or `Extensions/` subfolders inside a use case: its few files sit together. No `Public…` feature folders: `GetPublicRedirects` is a query in `Features/Redirects/Queries/`. The shared rule text is in the [module rules](../../standards/module-rules.md#folder-structure-inside-a-module).

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
- Each specification belongs to one use case and lives in its use-case folder. Do not create a shared `<Area>/Specifications/` or import a specification from another use case. If two use cases share the same simple condition, each gets its own local specification; the only exception is truly shared code in an explicitly named `Shared/`, after the need is confirmed.
- Put DTOs that other modules use in `<Module>.Contracts`, not next to a handler. A handler never returns another module's Contracts DTO as its own HTTP response.
