# Catalog sewing composition structure

## Purpose

Remediate the structural findings from the SDD002 implementation audit without changing its behavior, HTTP contract, or database schema.

## Accepted rules

- Follow `docs/sdd/standards/backend-rules.md`: one primary production type per file, one Mediator scenario per handler, and feature/use-case-oriented Application structure.
- Domain owns sewing aggregate invariants and the reusable readiness policy. Application assembles API responses from projected facts using that same policy; Infrastructure provides persistence adapters and query projections.
- Keep `Product` as the aggregate root. Keep generated EF migration and model snapshot files intact.
- Preserve routes, operation IDs, JSON property names/types, validation outcomes, blocker text, persistence semantics, and current schema. No new migration or OpenAPI change is authorized.

## Audit finding to remediation mapping

See the SDD002 [code audit](../002-sewing-articles-and-composition/code-audit/audit.md).

| Finding | Remediation |
| --- | --- |
| AF-01, AF-02 | RM-001: separate requests, handlers, and response types by scenario. |
| AF-03 | RM-002: keep readiness evaluation in a reusable Domain policy used by both the `Product` aggregate and projected-read assembler. |
| AF-04 | RM-003: separate tracked write persistence from AsNoTracking read projections. |
| AF-05 | RM-004: give sewing enum types their own files under Domain Enums. |
| AF-06 | RM-005: organize Application by use case with only genuinely shared contracts/helpers in `Shared`. |
| AF-07 | RM-006: separate Article and composition HTTP resource controllers while retaining all route names/policies. |

## Target ownership and paths

- Domain: `Products/Entities/Product.SewingComposition.cs` remains part of Product aggregate behavior; `Products/Readiness/SewingCompositionReadiness.cs` owns the shared readiness policy; enums have matching files in `Products/Enums`.
- Application: `Features/Product/Sewing/{CreateArticle,ListArticles,GetComposition,ReplaceComposition,ReplaceArticleComposition}` for scenario-owned requests, handlers, validators, and responses; shared types use `Shared/{DTOs,Validators,Mapping}`. Persistence interfaces and snapshots are under `Contracts/Persistence/Sewing`.
- Infrastructure: `Repositories/SewingCompositionStore.cs` for tracked writes; `Repositories/SewingCompositionReadStore.cs` for active/version/line projection snapshots. `CompositionReadRow.cs` is a separate read-model type. Application assembles blockers from that same snapshot through the Domain policy.
- Tests: separate domain, handler, and query/persistence concerns with their existing test project conventions.

## Verification and rollback

- Run focused Sewing and Product unit tests, Sewing API/persistence/query integration tests, Catalog architecture tests, and Host API compile.
- Run formatting checks on changed C# files and inspect the final diff to confirm no public contract, generated migration, schema, or unrelated SDD003 changes.
- If a focused check fails, fix it or leave the related remediation open with the concrete error. Roll back only the decomposition diff; database rollback is inapplicable because no database change is part of this plan.

## Status

Implementation is complete for RM-001 through RM-006. No route, operation ID, JSON DTO shape, migration, schema, or persistence behavior change is intended; no live database was accessed.

### Verification evidence (2026-10-25)

- `dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'FullyQualifiedName~SewingArticleTests|FullyQualifiedName~SewingCompositionHandlerTests|FullyQualifiedName~ProductCommandHandlerTests|FullyQualifiedName~ProductWriteRequestMapperTests' --verbosity quiet` — passed, 34 tests.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter 'FullyQualifiedName~SewingCompositionApiTests|FullyQualifiedName~SewingCompositionPersistenceTests|FullyQualifiedName~SewingCompositionQueryTests|FullyQualifiedName~ProductsApiTests' --verbosity quiet` — passed, 19 tests.
- `dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --verbosity quiet` — passed, 1 test.
- `dotnet build src/Bootstrapper/Host.Api/Host.Api.csproj --no-restore --verbosity quiet` — passed with zero warnings/errors on the final run.
- `dotnet format whitespace BreviERP.sln --include ... --no-restore --verbosity quiet` — passed for changed application, domain, infrastructure, host, and test paths.
- The readiness facts `MaterialReadinessFact` and `OperationReadinessFact` are separate Domain files and use typed `CompositionLineId`; `SewingCompositionReadiness` owns only the policy method.
- After the AF-03 snapshot/readiness correction, the Article/readiness/handler unit subset passed 17 tests, including aggregate/policy parity and successful-write blocker coverage; the focused Sewing API/persistence/query integration subset passed 11 tests.
- A preceding focused unit run reported `NU1903` for `Microsoft.OpenApi.YamlReader 2.7.5`; the final focused unit command passed. No PostgreSQL live database or migration run was performed.
