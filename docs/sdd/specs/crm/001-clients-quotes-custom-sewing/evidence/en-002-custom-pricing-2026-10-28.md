# EN-002 custom arithmetic and versioned input proof — 2026-10-28

**Status:** implemented and automatically verified. Scope: the remaining custom-calculation prerequisite for SC-S01/SC-S02; this is the first P4 checkpoint, not completed TS-006 or all P4. Backend starting HEAD: 8fa536bbde41ab60d197531ace3f1287ca8558c5. The backend tree was initially clean. Existing frontend FE-005 changes were preserved.

## Sources and owner

Compared the sanitized Calculator v211 lib/custom-sewing.ts against Catalog SewingArticleCalculator, SewingCostBreakdown, SewingCalculationSettings and the existing approved-engine tests. The source subtracts application internal cost from the sewing markup base, then adds separately approved application sale once to every tier. Catalog remains the formula owner; no browser or Crm formula copy.

Implemented Catalog Domain SewingCustomCalculator/charges/result, with the common SewingTierPricing extracted from the existing Article calculator. Existing Article signatures and results remain unchanged. Catalog Application SewingCustomPricingEvaluator accepts bounded selections/norms, resolves server-approved effective settings and Reference versions at one UTC instant through existing ports, and returns typed input/settings/reference/calculation evidence. It uses shared material/operation resolution without constructing a fake Product/Article. No HTTP operation, database schema, migration, storage adapter, dependency or public Catalog product was introduced.

## Reconciled arithmetic

Let base be the common engine full cost (materials, accessories, assembly-inclusive labour, role supplements and production overhead). Sewing markup base = base + delivery + packaging. Internal cost = sewing markup base + application cost. Each offered tier = AwayFromZero(sewing markup base × its approved multiplier + application sale, 2). Application cost is retained internally and does not additionally enter sewing markup; application sale is not multiplied. All arithmetic uses decimal, with existing Catalog unrounded component costs and a final two-place tier round. This deliberately follows the approved target engine rounding; legacy JavaScript per-component rounding and fallback constants are not copied. No extra VAT calculation.

Charges have two-place nonnegative amounts. Delivery/packaging are bounded by 10000000 each; aggregate application cost/sale by 200000000 (20 source-supported applications of up to 10000000 each). TS-006 must enforce individual application count/amount bounds before aggregation. Selection norms/time are positive, at most 100000 and at most six places; private material and operation counts are bounded at 140/100, with at most 40 fabric rows. Missing/ambiguous settings, unsupported formula, bad source versions, unapproved/inactive input, incompatible units and arithmetic overflow cannot produce a zero or partial price. No OP-155 shortcut or hardcoded coefficients.

## Synthetic golden fixture

Approved settings version 10/revision 1: salary 1000, work days 10, shift minutes 100, seamstresses 10, assembly 0.25, each of cutting/master/manager/bonus 0.1, monthly overhead 10000, tier factors 2/1.5/1.25. Reference versions: Fabric:1 version 101 at 100 per meter; Operation:2 version 102 at 10 minutes. Quantities are 1. Delivery/packaging 5/5, application cost/sale 12/30.

Labour = 12.5, role supplements = 5, overhead = 12.5, base cost = 130; markup base = 140, internal cost = 152; retail/wholesale/largeWholesale = 310 / 240 / 205. An independent fabric alternative at 200 gives internal cost 252 and tiers 510 / 390 / 330. Raising only application internal cost from 12 to 99 changes internal cost by 87 and leaves offered tiers unchanged. Tests also prove final decimal midpoint rounding and the 20-application aggregate boundary.

## Red → Green → Refactor → Regression

- Red: CustomSewingPricingTests, 9 failed / 0 passed against the initial base-engine delegation: custom charges/sale and invalid-charge blockers were absent. This was an executable behavioral failure, not a compilation failure.
- Green: custom plus existing Article tests, 22 passed.
- Adapter proof: versioned settings/reference ports and typed evidence use the prerequisite-analysis exception; validated by focused Application tests, without pretending this is persistence/API evidence. A test-fixture record/list equality mismatch was corrected to compare collection content; that mismatch was not counted as behavioral Red.
- Final regression: 41 passed (15 custom arithmetic, 13 input-resolution/Application, 13 existing Article tests). Separate boundary checks: 2 passed.

Commands from backend root:

```powershell
dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter FullyQualifiedName~CustomSewingPricingTests --verbosity quiet
dotnet test tests/UnitTests/UnitTests.csproj --no-restore --filter 'FullyQualifiedName~CustomSewingPricingTests|FullyQualifiedName~CustomSewingEvaluationTests|FullyQualifiedName~SewingArticlePricingTests' --verbosity quiet
dotnet test tests/ArchitectureTests/ArchitectureTests.csproj --no-restore --filter 'FullyQualifiedName~CatalogApplicationDependencyTests|FullyQualifiedName~CrmDependencyTests' --verbosity quiet
```

The first command is the recorded Red (exit 1); final regression and architecture commands exited 0. The existing UnitTests dependency warning NU1903 for Microsoft.OpenApi.YamlReader 2.7.5 remains outside this task; no package changed.

## Boundaries and next task

One primary production type per file; input/result contracts, calculation, validation and input resolution remain separated. Application depends on existing read ports and Domain, never EF or another module infrastructure. Evaluation is explicit, sequential and cancellation-aware; malformed reference keys fail before the Reference adapter can parse them. Previous typed results retain their original input/source versions when later calls see different costs.

TS-006 still owns private draft/version persistence, optimistic concurrency/idempotency, actor scope, server-held calculation tokens and quote hydration. TS-007/EN-003 owns real private bytes; TS-008 owns templates/suggestions; FE-006/007 follow the pinned API handoff. No stub API/storage or fictional immutable quote integration is claimed by this calculation prerequisite.

No working database, migration, publication, browser/manual acceptance, commit or push was performed. User instruction: manual acceptance is moved to the end and does not block the next ready implementation task. [Task](../tasks/readiness/en-002-price-basis-and-custom-arithmetic-reconciliation.md).

## Final documentation and review checks

Changed-backend Markdown link validation passed for 9 files; frontend Markdown checker passed for 499 files after the manual-acceptance instruction was recorded. Scoped Markdown formatting and git diff --check passed. Reviewed the two existing production diffs and all new calculation/contracts/validation files against adjacent code: no new transport, EF, module dependency, server state, secrets or external side effects. No additional automated run was needed for the documentation-only final pass.
