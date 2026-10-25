# SDD002 code audit

Audit status: findings verified after the SDD002 structural decomposition. Baseline evidence below points to the committed implementation before remediation. Structural remediation is tracked as EN-004 in `tasks.md` and implemented by the [Catalog sewing composition structure remediation SDD](../../004-sewing-composition-structure/README.md).

## AF-01 — Application contracts are grouped by file instead of scenario

- Severity: P2; status: verified.
- Baseline evidence: `src/Modules/Catalog/Catalog.Application/Catalog.Application/Features/Product/Sewing/CompositionModels.cs:5-22` declares ten request/view types in one file; `src/Modules/Catalog/Catalog.Application/Catalog.Application/Features/Product/Sewing/SewingRequests.cs:3-9` declares five command/query types. `docs/sdd/standards/backend-rules.md` requires one primary type per file and colocated scenario DTOs.
- Impact: unrelated operations share ownership and edits risk accidental coupling; filenames do not identify the contract.
- Owner: Catalog Application.
- Resolution: matching request/command/query and response files are under `Features/Product/Sewing/{CreateArticle,ListArticles,GetComposition,ReplaceComposition,ReplaceArticleComposition}`; cross-scenario DTOs, validators, and mappers have matching subfolders under `Shared`, while persistence contracts/snapshot live under `Contracts/Persistence/Sewing`.
- Dependencies: none.
- Verification: focused unit and integration tests, build Catalog Application and Host API.

## AF-02 — Read handlers are combined

- Severity: P2; status: verified.
- Baseline evidence: `src/Modules/Catalog/Catalog.Application/Catalog.Application/Features/Product/Sewing/SewingReadHandlers.cs:3-25` contains both list and composition query handlers, contrary to the one-handler-per-scenario rule.
- Impact: handler ownership and navigation are unclear.
- Owner: Catalog Application.
- Resolution: handlers are split into `ListArticles/ListSewingArticlesHandler.cs` and `GetComposition/GetSewingCompositionHandler.cs` beside their queries.
- Dependencies: AF-01.
- Verification: focused query tests and architecture test.

## AF-03 — Persistence adapter owns readiness business policy

- Severity: P1; status: verified.
- Baseline evidence: `src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Repositories/SewingCompositionStore.cs:28-40` constructs blockers for inactive articles, unreviewed lines, OtherCost ownership, missing fabric, and missing operations.
- Impact: Infrastructure defines scenario/business readiness and duplicates Domain responsibility, risking inconsistent results between loaded aggregates and projected reads.
- Owner: Catalog Application / Domain.
- Resolution: `SewingCompositionReadStore` returns active status, version, and projected lines together in a read snapshot. `SewingCompositionReadAssembler` and `Product.GetCalculationReadinessBlockers` both invoke the same Domain `SewingCompositionReadiness` policy, preserving blocker strings and avoiding a second tracked aggregate load.
- Dependencies: none.
- Verification: readiness unit and integration tests; query tests against the same cases.

## AF-04 — One store mixes writes and read projections

- Severity: P2; status: verified.
- Baseline evidence: `src/Modules/Catalog/Catalog.Infrastructure/Catalog.Infrastructure/Repositories/SewingCompositionStore.cs:12-73` loads tracked aggregates, checks SKU uniqueness, saves/translates persistence conflicts, and defines two AsNoTracking projection queries.
- Impact: persistence write and projection responsibilities are coupled, complicating query ownership and maintenance.
- Owner: Catalog Infrastructure.
- Resolution: `SewingCompositionStore` implements writes; `SewingCompositionReadStore` and `ISewingCompositionReadStore` own projections. Its `CompositionReadRow` is a separate top-level type file.
- Dependencies: AF-01.
- Verification: focused application/integration persistence and query tests.

## AF-05 — Domain sewing types are co-located

- Severity: P2; status: verified.
- Baseline evidence: `src/Modules/Catalog/Catalog.Domain/Catalog.Domain/Products/Entities/Sewing/SewingMaterialLine.cs:7-8` declares both enums alongside the entity.
- Impact: primary type/file mapping is ambiguous and enum ownership is obscured.
- Owner: Catalog Domain.
- Resolution: `Products/Enums/SewingMaterialKind.cs` and `SewingMaterialUnit.cs` each contain one enum; the entity stays in its own file.
- Dependencies: none.
- Verification: Domain and EF model tests; no schema change expected.

## AF-06 — Catalog Application sewing capability is a flat scenario collection

- Severity: P2; status: verified.
- Baseline evidence: `src/Modules/Catalog/Catalog.Application/Catalog.Application/Features/Product/Sewing/{CreateSewingArticleHandler,ReplaceSewingCompositionHandler,ReplaceSewingArticleCompositionHandler,SewingReadHandlers}.cs` were siblings, while common validation/factory helpers shared that flat directory.
- Impact: the structure obscures scenario boundaries and contradicts the required feature/use-case layout.
- Owner: Catalog Application.
- Resolution: the five operations have use-case directories with scenario validators; material and operation line validators have separate files under `Shared/Validators`. `CompositionLineFactory` remains one cohesive mapping type because both methods map the same shared composition line concepts; storage interfaces/snapshot are under `Contracts/Persistence/Sewing`.
- Dependencies: AF-01, AF-02.
- Verification: architecture test, focused tests, and API integration tests.

## AF-07 — Separate HTTP resource ownership

- Severity: P2; status: verified.
- Baseline evidence: `Catalog.Api/Controllers/SewingCompositionController.cs:13-109` combined Article lifecycle endpoints and composition read/replacement endpoints. The resource roles were independent even though the transport class had one type.
- Owner/target: Catalog API; `SewingArticlesController.cs` owns Article list/create and `SewingCompositionController.cs` owns composition operations. RM-006 records the structural task.
- Verification: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --no-restore --filter FullyQualifiedName~SewingCompositionApiTests --verbosity quiet` passed 5/5 after the split. Route names, policies, HTTP statuses and JSON bodies are preserved.

## Keep decisions

- Keep `Product` as the aggregate root for Article and composition rules; a cohesive `Product.SewingComposition.cs` partial is appropriate only if it preserves the same aggregate and invariant ownership.
- Keep `CompositionLineFactory` together as one cohesive mapper for the composition's two line kinds; split material and operation validators because they validate distinct request contracts.
- Keep generated EF migration and model snapshot files as generated artifacts; do not hand-split them.
- Preserve public routes, operation IDs, JSON property names and types, and database schema. No migration or OpenAPI change is in scope.
- Preserve unrelated working-tree documentation changes and the untracked SDD003 directory.

## Verification

- Initial structural checkpoint: 34 focused unit tests passed. Subsequent readiness regressions are covered by the later subset recorded below; the earlier count is preserved as historical evidence.
- Sewing API/persistence/query and Products API integration tests: 19 passed.
- Catalog architecture test: 1 passed; Host API build succeeded.
- Formatter and `git diff --check` passed. No live database or migration was used.
- After the AF-03 snapshot/readiness correction, the focused Article/readiness/handler unit subset passed 17 tests (including policy parity and successful-write blocker coverage), and the focused Sewing API/persistence/query integration subset passed 11 tests.
- The coordinator additionally verified the final 12 JSON-schema/aggregate OpenAPI tests with `--no-build --no-restore --filter "FullyQualifiedName~SewingCompositionContractTests|FullyQualifiedName~Aggregate_contract_is_valid|FullyQualifiedName~Aggregate_operation_ids_are_unique"`; all passed against the freshly compiled unit-test assembly. Final `Host.Api` build passed with zero warnings/errors.
