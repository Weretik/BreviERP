# Use-Case Flow

## Standard HTTP flow

```text
HTTP request
  → Controller binding
  → ISender.Send(request, cancellationToken)
  → Mediator pipeline
  → Command/query handler
  → Domain + Application abstractions
  → Infrastructure adapter / database
  → Ardalis.Result
  → ToActionResult HTTP response
```

## CQRS rules

- A command changes state and returns only the required identifier, state, or Result.
- A query does not change state and projects only the required fields.
- Do not hide writes in query handlers or background read operations.
- Pass `CancellationToken` through every asynchronous operation.
- Make retry-sensitive commands idempotent when duplicate side effects are possible. If an endpoint uses `Idempotency-Key`, document the key, scope, and retention.

## Feature placement

```text
<Module>.Application/Features/<Area>/<UseCase>/
  <UseCase>Command|Query.cs
  <UseCase>Command|QueryHandler.cs
  DTOs/                 only when private to this use case
  Validators/
  Extensions/           cohesive query/mapping helpers only
```

Split a file when its responsibilities evolve independently. Do not create generic dump folders such as `Common`, `Helpers`, or `Utils`.
