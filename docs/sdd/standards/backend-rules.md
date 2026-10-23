# Backend rules

## Layers and dependencies

Follow the `API → Application → Domain` dependency direction. Infrastructure depends on inner layers, never the reverse.

- Domain owns business invariants, entities, value objects, and domain services; it contains no transport DTOs or Infrastructure dependencies.
- Application orchestrates scenarios through abstractions, not Infrastructure implementations.
- Infrastructure contains persistence and integration adapters; it does not own business rules.
- API performs only HTTP binding, authorization, and result mapping; it contains no EF or business logic.

## Entity identifiers

Before creating or changing an entity identifier, follow the [identifier strategy](identifier-strategy.md). Do not use `Guid` automatically: the ID type depends on the creation and integration lifecycle. In Domain, wrap the chosen scalar value in a typed value object; do not use an unwrapped primitive ID in `BaseEntity`, `BaseAuditableEntity`, or Domain foreign keys.

## CQRS, Mediator, and code

Organize Application by feature. Keep a command/query, handler, validator, and private DTOs together by scenario. One Mediator handler owns one scenario; commands change state, while queries do not.

Pass `CancellationToken` through the entire execution flow. Keep cross-cutting concerns in pipeline behaviors; do not move business rules into those behaviors.

### Ardalis.Result

- Use `Ardalis.Result` for expected scenario outcomes rather than custom success/error wrappers. For a paginated list query, the default result is `PagedResult<IReadOnlyList<TItem>>` with `PagedInfo`; do not create a local DTO that duplicates page, page-size, and total-count metadata.
- Use `Map` for a pure conversion from `Result<TSource>` to `Result<TTarget>`. For a sequence of operations that each return `Result`, use `Bind`/`BindAsync` when they remove repeated status checks. Do not use them to hide write side effects or important database and state checks.

Use explicit names such as `CreateOrderCommand`, `GetPublicProductListQuery`, and `GetAdminProductListQueryHandler`. Do not create generic directories or names such as `Common`, `Misc`, `Helper`, `Utils`, or `Manager`. Do not add abstractions without a concrete need.

## File structure quality

- One production code file has one primary responsibility and generally one top-level type. Do not combine multiple DTOs, specifications, handlers, or unrelated helper types in one file merely to reduce file count.
- Organize code by feature: command/query, handler, validator, DTO, and specification belong to a particular scenario. Always keep specifications in `<UseCase>/Specifications/`; a generic `<Area>/Specifications/` directory or a specification reused from another scenario is forbidden. `Shared` is allowed only for genuinely shared Product/feature code, not as a dumping ground for unstructured types.
- Match the filename to its public top-level type: `CreateProductCommand.cs`, `CreateProductCommandHandler.cs`, `ProductByIdSpec.cs`, and so on.
- Follow the project's existing formatting. Do not use compressed one-line style for namespaces, classes, handlers, validators, or complex DTOs. Before completing a change, run the formatter or compare the diff with adjacent files.
- Investigate existing conventions, contracts, and dependencies first, then make the smallest complete implementation. Temporary stubs, partial scaffolds, and premature completion claims are forbidden when they cannot close the task and be verified.
- Do not duplicate a capability of an already installed library with a custom wrapper or utility until you have checked that its type or extension does not cover the agreed scenario.

## Domain events and transactions

Follow the existing domain event lifecycle: `collect → dispatch → clear`. The current code has no shared `IUnitOfWork`. For a scenario changing multiple aggregates, define the transaction boundary, possibility of partial completion, and verification in the specification; do not assume automatic atomicity across contexts.
