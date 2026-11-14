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

There are none: no module raised one, so the dispatch behavior was removed in stage 3c. A change that another module or the outside world must learn about goes through the [transactional outbox](../../standards/module-rules.md#transactional-outbox).

## Observability and security

Use structured Serilog templates without secrets, tokens, connection strings, or sensitive request data. Preserve correlation and cancellation. Authorization is configured through host policies; an anonymous endpoint needs an explicit `AllowAnonymous` because the host has a fallback policy requiring authentication.
