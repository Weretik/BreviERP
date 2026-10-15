# Потік сценарію

## Стандартний HTTP-потік

```text
HTTP request
  → Controller binding
  → ISender.Send(request, cancellationToken)
  → Mediator pipeline
  → Command/query handler
  → Domain + абстракції Application
  → Infrastructure adapter / database
  → Ardalis.Result
  → ToActionResult HTTP response
```

## Правила CQRS

- Команда змінює стан і повертає лише потрібний ідентифікатор, стан або Result.
- Запит не змінює стан і проєктує лише потрібні поля.
- Не приховуйте записи в обробниках запитів або фонових операціях читання.
- Передавайте `CancellationToken` через кожну асинхронну операцію.
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
