# Colours («Кольори»)

**Module:** reference. **Spec:** [REF-006](../../specs/reference/006-fabric-colors/README.md). **Updated:** 2026-11-11.

The colours of the client sheet «Тканини», column «Загальний перелік кольорів» (61 at the start, added by the migration), with an active flag and list order; the tab «Кольори» of «Матеріали» edits them. The workshop picks an item's main and additional colours and the colour of a new stock line only from the active colours; a colour already saved on the record stays valid. Records keep the colour text, so a rename does not change them.

## Code Structure

```text
src/Modules/Reference/
├── Reference.Api/Reference.Api/
│   └── Endpoints/FabricColors/                      # /api/reference/fabric-colors, one <UseCase>/ folder each
├── Reference.Application/Reference.Application/Features/FabricColor/
│   ├── Commands/{Create,Update,Delete}FabricColor/  # CRUD, FluentValidation
│   ├── Queries/GetFabricColors/                     # list
│   └── Shared/                                      # fields, validator, duplicate key, spec
├── Reference.Domain/Reference.Domain/FabricColors/  # entity, details, errors, id
└── Reference.Infrastructure/Reference.Infrastructure/
    ├── Persistence/Configurations/Materials/FabricColorConfiguration.cs
    └── Persistence/Migrations/*_AddFabricColors.cs              # table + the 61 sheet colours
src/Modules/Production/Production.Application/Production.Application/
├── Abstractions/Materials/IProductionColorReader.cs
└── Features/Colors/                                 # Shared/ProductionColors check, Queries/GetProductionColors
src/Modules/Production/Production.Infrastructure/Production.Infrastructure/Adapters/Reference/ProductionReferenceReaders.cs   # ProductionColorReader: Reference.Contracts IFabricColorDirectory, active only
```

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~FabricColorTests|FullyQualifiedName~ProductionColorTests"`.

**Data:** [reference schema](../../architecture/data-model/reference.md)

## Related Sources

[OpenAPI](../../contracts/reference/fabric-colors.openapi.yaml) · [Reference boundaries](../../architecture/overview/modules.md).
