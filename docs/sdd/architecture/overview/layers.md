# Layer Boundaries

## Domain Layer

Owns aggregates, entities, value objects, domain errors, and domain events. Business invariants belong here and must be enforced by constructors, factories, or behavior methods. Domain must not depend on transport DTOs, EF, HTTP, logging, configuration, or Infrastructure.

Use value objects for validated conceptual identifiers and values. Do not expose an entity as a public request/response model.

## Application Layer

Owns use cases, commands, queries, handlers, validators, and abstractions implemented by adapters. Each Mediator handler implements one use case. Commands change state; queries do not and should use untracked read projections where possible.

Application returns Ardalis.Result contracts. It defines persistence and integration interfaces, such as read contexts or repositories, but never creates their implementations.

## Infrastructure Layer

Owns EF Core DbContexts, entity configurations, migrations, repository implementations, external clients, persisted-data projections, and adapter registrations. It may depend on inner layers; inner layers may not depend on it.

## API Layer

Controllers are thin transport adapters: they bind input, send one Mediator request with a cancellation token, and map Result using established API conventions. Controllers contain no business rules, EF queries, or transaction orchestration.

## Host Composition (Bootstrapper)

Hosts compose modules, configure middleware, and register cross-cutting services. They are not a place for use-case logic.
