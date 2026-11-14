# Mediator Behaviors

The host registers these pipeline behaviors for all Mediator requests in this exact order:

```text
ISender.Send(...)
  │
  ├── 1. RequestLoggingBehavior
  ├── 2. PerformanceBehavior
  ├── 3. ValidationBehavior
  ├── 4. ExceptionBehavior
  └── Handler
```

## 1. RequestLoggingBehavior

Logs the start, completion, and failure context of a Mediator request using existing structured logging conventions. This provides observability only; the behavior does not change business logic or log sensitive request data.

## 2. PerformanceBehavior

Measures request duration using configured `PerformanceBehavior` settings and emits diagnostics for slow requests. It does not change request or command semantics.

## 3. ValidationBehavior

Runs all FluentValidation validators for a request before the handler. For `Result` or `Result<T>` responses, validation failures become `Result.Invalid`; the handler is not called. Validators check input contracts, not aggregate state rules.

## 4. ExceptionBehavior

Logs critical failures and turns a unique-key violation into `Conflict`; every other exception goes on to `GlobalExceptionHandler` (500). Business outcomes are returned as `Error`/`Result`, not exceptions.

## Rules for new behaviors

- Add a behavior only for a genuinely cross-cutting need that applies to multiple use cases.
- Keep the order intentional; document any change in an ADR and the relevant specification.
- Never put domain policy, endpoint-specific authorization, or use-case orchestration in a behavior.
