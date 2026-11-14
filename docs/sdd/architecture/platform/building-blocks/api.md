# BuildingBlocks.Api

## Purpose

BuildingBlocks.Api contains the MVC mapping from Ardalis.Result to HTTP ActionResult.

## Mapping

```text
ResultStatus.Ok           → 200 OK with value
ResultStatus.NotFound     → 404
ResultStatus.Invalid      → 400 with validation errors
ResultStatus.Conflict     → 409 with errors
ResultStatus.Forbidden    → 403
ResultStatus.Unauthorized → 401
other/unmapped            → 500
```

## Rule

Minimal API endpoints call the shared `ToHttpResult` / `ToNoContentHttpResult` extension methods (`BuildingBlocks.Api/Result/ResultToHttpResult.cs`) after `ISender.Send`. Public site groups use `WithPublicResponseCache` (`BuildingBlocks.Api/Caching/`). They do not reinterpret Result statuses manually for each endpoint unless the documented API contract requires an existing extension defined by project rules.
