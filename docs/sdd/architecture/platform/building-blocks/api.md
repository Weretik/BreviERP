# BuildingBlocks.Api

## Призначення

BuildingBlocks.Api містить MVC mapping від Ardalis.Result до HTTP ActionResult.

## Мапінг

```text
ResultStatus.Ok           → 200 OK зі значенням
ResultStatus.NotFound     → 404
ResultStatus.Invalid      → 400 з validation errors
ResultStatus.Conflict     → 409 з errors
ResultStatus.Forbidden    → 403
ResultStatus.Unauthorized → 401
інше/не зіставлене        → 500
```

## Правило

Контролери викликають усталений extension `ToActionResult` після `ISender.Send`. Вони не переінтерпретують Result statuses вручну для кожного endpoint-а, окрім випадку, коли задокументований API contract потребує наявного project convention extension.
