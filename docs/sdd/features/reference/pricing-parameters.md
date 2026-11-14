# Pricing Parameters («Параметри ціноутворення»)

**Module:** reference. **Spec:** [REF-004](../../specs/reference/004-pricing-parameters-sheet/README.md). **Updated:** 2026-11-02.

The Calculator sheet «Налаштування цін» in Brevi. Two tables:

- **Parameters** (`PricingParameter`): salary, work time, production rates, overhead and price multipliers, keyed by an upper-case `Code` (`SALARY_SEAMSTRESS`…) that never changes. Percentages are fractions (0.25 = 25 %). A calculated parameter (`IsCalculated`) has no stored value: the list computes it from [PricingParameterFormulas](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/PricingParameters/PricingParameterFormulas.cs) (`MINUTE_COST_SEAMSTRESS` = salary / (days × shift minutes)).
- **PPE markup rules** (`PpeMarkupRule`): retail / wholesale / large wholesale markups per PPE group; the `Name` is the rule code PPE offers use and never changes.

Editing a value sets «Дата оновлення» to today (Kyiv). The sheet is moved once through `POST /api/reference/pricing-parameters/import` (preview by default, `deleteMissing` removes rows not in the sheet). The rows are prepared by `brevi-calculator-references/transfer/price_settings_rows.py` in the project files.

Old PPE products may still refer to a parameter `Id` for their retail/wholesale percent (`AdditionalReferenceItem` in Catalog); the Catalog reader maps parameters to that shape. Every saved change is handed to Catalog (`IPricingSettingsChangeNotifier` → Catalog.Contracts `IPricingSettingsSync` → Catalog `SyncPricingSettingsCommand`): a changed value appends a new sewing settings version or PPE rule version, so prices and the «Операції» minute cost follow this page (REF-004 TS-04).

## Code Structure

```text
src/Modules/Reference/
├── Reference.Api/Reference.Api/
│   ├── Endpoints/PricingParameters/                 # CRUD + sheet import, one <UseCase>/ folder each with its request
│   └── Endpoints/PpeMarkupRules/                    # CRUD, one <UseCase>/ folder each with its request
├── Reference.Application/Reference.Application/Features/
│   ├── PricingParameter/{Commands,Queries,Shared}/   # Create/Update/DeletePricingParameter, GetPricingParameters
│   ├── PpeMarkupRule/{Commands,Queries,Shared}/      # Create/Update/DeletePpeMarkupRule, GetPpeMarkupRules
│   └── PricingSettingsImport/                       # Commands/ImportPricingSettings: both tables, preview and apply
├── Reference.Domain/Reference.Domain/
│   ├── PricingParameters/                           # entity, details, formulas, errors, id
│   └── PpeMarkupRules/                              # entity, details, errors, id
└── Reference.Infrastructure/Reference.Infrastructure/
    ├── Persistence/{PricingParameters,PpeMarkupRules}/*Configuration.cs
    └── Persistence/Migrations/*_PricingSettingsFromSheet.cs     # renames AdditionalReferences, keeps old rows

src/Modules/Reference/.../Abstractions/IPricingSettingsChangeNotifier.cs        # port called after each save
src/Modules/Reference/.../Adapters/Catalog/PricingSettingsChangeNotifier.cs  # port → Catalog.Contracts IPricingSettingsSync
src/Modules/Catalog/.../Contracts/Pricing/PricingSettingsSync.cs               # reads both Reference.Contracts lists, sends Catalog sync
src/Modules/Catalog/.../Features/Pricing/Commands/SyncPricingSettings/           # sheet codes → sewing settings, PPE rule versions
```

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~PricingSettingsTests|FullyQualifiedName~PricingReferenceQueryHandlerTests|FullyQualifiedName~PricingSettingsSyncTests"`; integration `PricingReferenceQueryIntegrationTests`.

**Data:** [reference schema](../../architecture/data-model/reference.md)

## Related Sources

[OpenAPI](../../contracts/reference/pricing-parameters.openapi.yaml) · [Reference boundaries](../../architecture/overview/modules.md).
