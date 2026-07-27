# Потік сценарію

## Стандартний HTTP-потік

```text
HTTP request
  → Controller binding
  → ISender.Send(request, cancellationToken)
  → Mediator pipeline
  → Command/query handler
  → Domain + application abstractions
  → Infrastructure adapter / database
  → Ardalis.Result
  → ToActionResult HTTP response
```

## Правила CQRS

- Command змінює стан і повертає лише потрібний identifier, status або Result.
- Query не змінює стан і проєктує лише потрібні поля.
- Не приховуйте writes у query handlers або background read operations.
- Поширюйте `CancellationToken` через кожну async operation.
- Робіть команди, чутливі до retry, ідемпотентними, коли можливі дубльовані побічні ефекти. Якщо endpoint використовує `Idempotency-Key`, документуйте ключ, scope і retention.

## Розміщення функціональності

```text
<Module>.Application/Features/<Area>/<UseCase>/
  <UseCase>Command|Query.cs
  <UseCase>Command|QueryHandler.cs
  DTOs/                 лише якщо приватні для сценарію
  Validators/
  Extensions/           лише цілісні query/mapping helpers
```

Розділяйте файл, коли його відповідальності розвиваються незалежно. Не створюйте універсальні папки-звалища, як `Common`, `Helpers` або `Utils`.
