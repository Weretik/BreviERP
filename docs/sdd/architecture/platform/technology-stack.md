# Technology Stack

| Area | Current choice | Rule for new work |
| --- | --- | --- |
| Runtime | .NET 10 / C# | Follow nullable annotation rules and configured analyzers. |
| HTTP | ASP.NET Core controllers | Keep controllers thin and use Mediator. |
| Mediator | Mediator 3.x | Use request/handler pairs and the existing pipeline. |
| Mediator pipeline | RequestLogging, Performance, Validation, Exception, DomainEventDispatcher | Preserve registration order; do not put business rules in pipeline behaviors. |
| Validation | FluentValidation | Place input validators alongside the use case. |
| Result contract | Ardalis.Result | Return `Result` or `Result<T>`; use the established API mapping. |
| Data access | EF Core 10 + Npgsql/PostgreSQL | Keep EF configuration and migrations in Infrastructure. |
| Repositories | Ardalis.Specification | Reuse the existing repository and specification style where applicable. |
| Identity | ASP.NET Core Identity | Preserve role/policy rules and fallback authentication. |
| Logging | Serilog | Use structured logs only and exclude sensitive data. |
| Background jobs | `AddBackgroundJobRegistrations()` is currently empty; the scheduler and job service are not implemented | Define the contract and execution approach in the specification first; do not bind Application directly to a scheduler. |
| Catalog media storage | `AdmToolsMediaStorageService` | Keep the external adapter behind the Infrastructure boundary. |
| Tests | NUnit projects for unit, integration, and architecture tests | Add tests in proportion to the changed boundary. |

Do not introduce a package to replace an existing project convention without an explicit decision in the feature specification and approval.

The [NuGet package catalog](nuget-packages.md) lists all direct dependencies, their versions, and owning projects. The `.csproj` files define the actual versions; update the catalog in the same change when a dependency changes.

## Tooling Outside NuGet

- Projects in `src/` and `tests/` specify `TargetFramework=net10.0`; both host Dockerfiles use .NET `10.0` images. The repository has no `global.json`, so the exact SDK patch level is not pinned here.
- `BreviERP.sln` is the entry point for `dotnet restore`, `dotnet build`, and `dotnet test`; current commands are listed in the [testing rules](../../standards/testing-rules.md).
- The two host Dockerfiles and `cloudbuild.yaml` define image builds and the Cloud Run Job → API service order. Verified configuration facts and limits are described in the [deployment guide](../../operations/deployment/cloud-build.md). This overview does not confirm the actual state of the external cloud environment.
