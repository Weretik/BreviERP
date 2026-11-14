# Workshop references («Швейний цех», stage 1)

The head of the workshop (role `WorkshopChief`), Admin and Director keep the workers of the workshop and fabric norms per Catalog sewing model, and see the composition of every model («Комплектація моделей»: lining, insulation, fleece and the full composition, read from Catalog). Payroll coefficients are shown only in «Параметри ціноутворення». Workers do not sign in. [SDD](../../specs/production/001-workshop-references/README.md), [norms and compositions](../../specs/production/009-fabric-norms-and-compositions/README.md), [contract](../../contracts/production/workshop-references.openapi.yaml), [tables](../../architecture/data-model/production.md).

## Code structure

```text
src/Modules/Production/
  Production.Domain/Production.Domain/
    Staff/WorkshopEmployee.cs          # worker; archived instead of deleted
    FabricNorms/                       # ModelFabricNorm (per model code), FabricNormValues (metres, totals, checks), ModelLayer (lining/insulation/fleece)
    ProductionText.cs                  # trims spaces, name key
  Production.Application/Production.Application/
    Abstractions/                      # IProductionDbContext, IProductionModelReader (Catalog), IPayrollParameterReader (Reference)
    Features/Staff/                    # GetWorkshopStaff, Create/UpdateWorkshopEmployee, SetWorkshopEmployeeActive, ImportWorkshopStaff
    Features/FabricNorms/              # GetFabricNorms, SaveFabricNorm, DeleteFabricNorm, ImportFabricNorms, GetModelCompositions; Shared/ validator, messages
    Features/PayrollParameters/Shared/ # PayrollParameterCodes (read by payroll and forecast)
  Production.Infrastructure/Production.Infrastructure/
    Persistence/                       # ProductionDbContext (schema production), configurations, migrations
    DependencyInjection/
  Production.Api/Production.Api/
    Staff/WorkshopStaffController.cs             # /api/production/staff
    FabricNorms/FabricNormsController.cs         # /api/production/fabric-norms, /compositions
src/Modules/Production/Production.Infrastructure/Production.Infrastructure/Adapters/Catalog/ProductionModelReader.cs  # IProductionModelReader → Catalog.Contracts ICatalogProductReader, ISewingModelReader
src/Modules/Production/Production.Infrastructure/Production.Infrastructure/Adapters/Reference/ProductionReferenceReaders.cs  # PayrollParameterReader → Reference.Contracts IPricingParameterDirectory
```

## Structure diagram

```mermaid
flowchart LR
  Admin[Brevi Admin «Швейний цех»] --> Controllers[Production controllers]
  Controllers --> UseCases[Mediator use cases]
  UseCases --> Db[(production schema)]
  UseCases --> Models[IProductionModelReader]
  UseCases --> Payroll[IPayrollParameterReader]
  Models -.Catalog.Contracts.-> Catalog[Catalog product overview and compositions]
  Payroll -.Reference.Contracts.-> Reference[Reference pricing parameters]
```

## Tests

- `tests/UnitTests/Production/` — staff, norm and composition handlers on SQLite in memory with fake Catalog models and pricing parameters.
- `tests/ArchitectureTests/ProductionDependencyTests.cs` — layer references, own schema and history, no module references Production.

Focused run: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~UnitTests.Production`.

**Data:** [production schema](../../architecture/data-model/production.md)
