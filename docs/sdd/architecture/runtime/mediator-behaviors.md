# Mediator Behaviors

The host registers these pipeline behaviors for all Mediator requests in this exact order:

```text
ISender.Send(...)
  │
  ├── 1. RequestLoggingBehavior
  ├── 2. PerformanceBehavior
  ├── 3. ValidationBehavior
  ├── 4. ExceptionBehavior
  ├── 5. DomainEventDispatcherBehavior
  └── Handler
```

## 1. RequestLoggingBehavior

Logs the start, completion, and failure context of a Mediator request using existing structured logging conventions. This provides observability only; the behavior does not change business logic or log sensitive request data.

## 2. PerformanceBehavior

Measures request duration using configured `PerformanceBehavior` settings and emits diagnostics for slow requests. It does not change request or command semantics.

## 3. ValidationBehavior

Runs all FluentValidation validators for a request before the handler. For `Result` or `Result<T>` responses, validation failures become `Result.Invalid`; the handler is not called. Validators check input contracts, not aggregate state rules.

## 4. ExceptionBehavior

Provides the established exception handling path around a request. Expected business outcomes are still represented through Ardalis.Result, not exceptions.

## 5. DomainEventDispatcherBehavior

Runs after a successful handler. It retrieves collected events from `IDomainEventContext`, clears them, and publishes each through `IDomainEventDispatcher`/Mediator. Entities collect events; controllers and handlers do not bypass this lifecycle.

## Rules for new behaviors

- Add a behavior only for a genuinely cross-cutting need that applies to multiple use cases.
- Keep the order intentional; document any change in an ADR and the relevant specification.
- Never put domain policy, endpoint-specific authorization, or use-case orchestration in a behavior.
