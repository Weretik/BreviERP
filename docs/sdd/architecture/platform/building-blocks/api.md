# BuildingBlocks.Api

## Призначення

BuildingBlocks.Api містить MVC mapping від Ardalis.Result до HTTP ActionResult.

## Мапінг

```text
ResultStatus.Ok           → 200 OK зі значенням
ResultStatus.NotFound     → 404
ResultStatus.Invalid      → 400 з валідації errors
ResultStatus.Conflict     → 409 з errors
ResultStatus.Forbidden    → 403
ResultStatus.Unauthorized → 401
інше/не зіставлене        → 500
```

## Правило

Контролери викликають усталений метод розширення `ToActionResult` після `ISender.Send`. Вони не переінтерпретують стани Result вручну для кожної кінцевої точки, окрім випадку, коли задокументований контракт API потребує наявного розширення, визначеного правилами проєкту.
