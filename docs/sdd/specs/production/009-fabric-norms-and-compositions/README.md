# PRD-009 «Норми витрати тканини»: the old «Цех» table and «Комплектація моделей»

Status: implemented 2026-11-11. Changes [PRD-001](../001-workshop-references/README.md). Admin: brevi-web spec `docs/specs/admin/production/009-fabric-norms-and-compositions`.

## Goal

The page looks and reads like «Витрата тканини за моделями» of the old «Цех»: each colour layout in its own columns with totals, the season under the model, and a second tab «Комплектація моделей» with the lining, insulation and fleece of each model and its full composition. «Правила зарплати» is removed: the coefficients are kept and shown in «Параметри ціноутворення» only.

## Decisions

| ID | Decision |
| --- | --- |
| PRD-D59 | `getFabricNorms` and `saveFabricNorm` rows carry `productId` and `season` from Catalog (the product overview), so the page shows «MOD-025 · Літо» and links to the model card. |
| PRD-D60 | «Комплектація моделей» has no copy of its own: `getModelCompositions` reads the composition of every sewing model from Catalog in one request (`ListSewingCompositionsQuery`, one split query over all sewing products, reference codes and names added). It is read-only; «Змінити комплектацію» opens the model card. Access is the page `fabric-norms`. |
| PRD-D61 | An extra layer is a composition line whose material name starts with «Підкладк» (Підкладка), «Синтепон» or «Утеплювач» (Утеплювач), «Фліс» (Фліс): `ModelLayer.PurposeOf`. The client sheet «Додаткові матеріали моделей» lists exactly these materials (FUR-021, FUR-022/023/024, FUR-026). The automatic fabric reservation still covers only the top fabric. |
| PRD-D62 | `access:payroll-rules`, its «Права доступу» row and `getPayrollParameters` are removed (owner 2026-11-11: the page only repeated «Параметри ціноутворення»). Saved grants for the key are ignored because `AccessCatalog.Find` no longer knows it. Payroll keeps reading the coefficients through `IPayrollParameterReader`. |
| PRD-D63 | The client sheet «розхід тканини по моделям» (copy of 2026-11-11) has 124 models; Brevi had 110 from the old «Цех» export. The missing 14 and MOD-095 (3.9 → 3.5 m) are loaded through the existing `importFabricNorms` (preview first, saved on the owner's «да»). |

## Acceptance

| ID | Check |
| --- | --- |
| PRD-009-A1 | Norm rows carry the Catalog product id and season. |
| PRD-009-A2 | Every Catalog sewing model is listed in `getModelCompositions`, also without composition; variant fabrics come first; lining and insulation are recognised as layers, threads are not. |
| PRD-009-A3 | Catalog composition lines carry FAB-/FUR- codes and reference names. |
| PRD-009-A4 | No controller needs `access:payroll-rules`; «Права доступу» has no «Правила зарплати». |

## File-role map

| Role | Files |
| --- | --- |
| Domain | `Production.Domain/FabricNorms/ModelLayer.cs` |
| Use cases | `FabricNorms/GetCompositions/GetModelCompositionsQuery.cs`, `GetModelCompositionsQueryHandler.cs`; `GetNorms`, `SaveNorm` handlers (product id, season) |
| Contracts | `Contracts/ModelCompositionContracts.cs`, `Contracts/FabricNormContracts.cs` |
| Catalog port | `Abstractions/IProductionModelReader.cs` (`ListCompositionsAsync`); host adapter `Host.Api/Production/ProductionModelReader.cs` |
| Catalog | `Contracts/Persistence/Sewing/ISewingCompositionListReader.cs`, `Features/Product/Sewing/ListCompositions/`, `Catalog.Infrastructure/Repositories/SewingCompositionReadStore.cs` (`ReadAllAsync`) |
| API | `Production.Api/FabricNorms/FabricNormsController.cs` (`compositions`) |
| Removed | `PayrollParametersController`, `GetPayrollParameters`, `PayrollParameterResponse`, `AccessCatalog.PayrollRules` |
| Contract | `docs/sdd/contracts/production/workshop-references.openapi.yaml` |

## Evidence

See the PR description.
