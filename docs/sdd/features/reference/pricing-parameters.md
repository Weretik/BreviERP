# Pricing Parameters («Параметри ціноутворення»)

**Module:** reference. **Spec:** [REF-004](../../specs/reference/004-pricing-parameters-sheet/README.md). **Updated:** 2026-11-02.

The Calculator sheet «Налаштування цін» in Brevi. Two tables:

- **Parameters** (`PricingParameter`): salary, work time, production rates, overhead and price multipliers, keyed by an upper-case `Code` (`SALARY_SEAMSTRESS`…) that never changes. Percentages are fractions (0.25 = 25 %). A calculated parameter (`IsCalculated`) has no stored value: the list computes it from [PricingParameterFormulas](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/PricingParameters/PricingParameterFormulas.cs) (`MINUTE_COST_SEAMSTRESS` = salary / (days × shift minutes)).
- **PPE markup rules** (`PpeMarkupRule`): retail / wholesale / large wholesale markups per PPE group; the `Name` is the rule code PPE offers use and never changes.

Editing a value sets «Дата оновлення» to today (Kyiv). The sheet is moved once through `POST /api/reference/pricing-parameters/import` (preview by default, `deleteMissing` removes rows not in the sheet). The rows are prepared by `brevi-calculator-references/transfer/price_settings_rows.py` in the project files.

Old PPE products may still refer to a parameter `Id` for their retail/wholesale percent (`AdditionalReferenceItem` in Catalog); the Catalog reader maps parameters to that shape. Catalog sewing settings and PPE rule versions do not read these tables yet (REF-004 TS-04).

## Code Structure

```text
src/Modules/Reference/
├── Reference.Api/Reference.Api/
│   ├── Controllers/PricingParametersController.cs   # parameters CRUD + sheet import
│   ├── Controllers/PpeMarkupRulesController.cs      # PPE markup rules CRUD
│   └── Contracts/{PricingParameters,PpeMarkupRules}/
├── Reference.Application/Reference.Application/Features/
│   ├── PricingParameter/{Create,Update,Delete,GetList,Shared}/
│   ├── PpeMarkupRule/{Create,Update,Delete,GetList,Shared}/
│   └── PricingSettingsImport/                       # import of both tables, preview and apply
├── Reference.Domain/Reference.Domain/
│   ├── PricingParameters/                           # entity, details, formulas, errors, id
│   └── PpeMarkupRules/                              # entity, details, errors, id
└── Reference.Infrastructure/Reference.Infrastructure/
    ├── Congigurations/{PricingParameter,PpeMarkupRule}Configuration.cs
    └── Migrations/*_PricingSettingsFromSheet.cs     # renames AdditionalReferences, keeps old rows
```

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~PricingSettingsTests|FullyQualifiedName~PricingReferenceQueryHandlerTests"`; integration `PricingReferenceQueryIntegrationTests`.

**Data:** [reference schema](../../architecture/data-model/reference.md)

## Related Sources

[OpenAPI](../../contracts/reference/pricing-parameters.openapi.yaml) · [Reference boundaries](../../architecture/overview/modules.md).
