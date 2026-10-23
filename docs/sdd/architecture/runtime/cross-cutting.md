# Cross-Cutting Pipeline

## Mediator request handling

Host.Api scans the Identity, Catalog, Reference, and BuildingBlocks Application assemblies. The configured pipeline order is:

1. request logging;
2. performance measurement;
3. FluentValidation;
4. exception handling;
5. domain-event dispatch after a successful handler.

All use-case entry points go through Mediator unless a module has a clearly documented exception.

## Validation and errors

Validators live beside the use case under `Validators` and validate input shape: required values, ranges, enum values, string length, and consistent combinations.

Domain code checks business invariants and state transitions. Do not duplicate a rule across layers without an intentional defensive reason.

`ValidationBehavior` converts validation failures to `Ardalis.Result.Invalid` when the response is `Result` or `Result<T>`. Controllers keep the existing mapping from results to HTTP.

## Domain events

Entities collect events through `IHasDomainEvents`. Dispatcher behavior collects, clears, and publishes them through Mediator after a successful handler. Follow the `collect → dispatch → clear` lifecycle; do not publish events directly from controllers.

## Observability and security

Use structured Serilog templates without secrets, tokens, connection strings, or sensitive request data. Preserve correlation and cancellation. Authorization is configured through host policies; an anonymous endpoint needs an explicit `AllowAnonymous` because the host has a fallback policy requiring authentication.
