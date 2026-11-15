# ADR-0007: Every use case is a command or query sent through Mediator from a Minimal API endpoint

**Status:** accepted · **Date:** 2026-11-15 (recorded from the current code) · **Owner:** Виталий (project owner)

## Context

Brevi has more than 400 use cases in thirteen modules: pricing, КП, production, warehouse, accounting and more. Each needs the same steps around it: logging, timing, input validation, error handling. The API started with controllers that grew several actions each; logic leaked into them, and the same steps were written by hand.

## Options

- Controllers with services per area: rejected; controllers and services grew without limit.
- Minimal API endpoints that call services directly: rejected; validation and logging would still be repeated.
- Minimal API endpoint per use case plus the source-generated `Mediator` with pipeline behaviors: chosen.

## Decision

- Every use case is a command (changes data) or a query (reads data) with one handler, in `Application/Features/<Feature>/Commands|Queries/<UseCase>/` next to its validator and response.
- Every endpoint is a Minimal API endpoint in `Api/Endpoints/<Feature>/<UseCase>/`. It maps its own HTTP request type to a separate command or query, sends it through `Mediator` 3.x and maps the result through the one mapper ([ADR-0008](0008-result-and-typed-errors.md)).
- Pipeline behaviors run in a fixed order: request logging, performance, validation, exception ([mediator behaviors](../runtime/mediator-behaviors.md)).
- A command handler only orders the steps: load, check what needs outside data, call the aggregate's method, one `SaveChanges`, return an id or a DTO.

## Consequences

- Every use case has the same shape and is found by its folder.
- Cross-cutting steps are written once.
- A small use case still needs several files; the pipeline order must be kept.
- [API rules](../../standards/api-rules.md) still describe controllers in places; the code has none (checked 2026-11-15), so those sections are to be updated.

## Sources

[Use-case flow](../runtime/use-case-flow.md), [module rules](../../standards/module-rules.md), [technology stack](../platform/technology-stack.md). Architecture tests check folder names and references (`tests/ArchitectureTests`).
