# Product sections («Типи і розділи»)

**Module:** reference. **Spec:** [REF-005](../../specs/reference/005-product-sections-sheet/README.md). **Updated:** 2026-11-03.

The Calculator sheet «Типи і розділи» in Brevi: product sections («Одяг ЛІТО», «Захист рук»…), each of one product type (`Sewing` or `Ppe`), with an active flag, comment and display order. Products will refer to a section by ID, and the public site will build its categories from sections. The sheet is moved once through `POST /api/reference/product-sections/import` (preview by default, `deleteMissing` removes rows not in the sheet), matched by name with case and punctuation ignored.

## Code Structure

```text
src/Modules/Reference/
├── Reference.Api/Reference.Api/
│   └── ProductSection/                              # ProductSectionsController (CRUD + sheet import) and its requests
├── Reference.Application/Reference.Application/Features/ProductSection/
│   ├── Commands/{Create,Update,Delete}ProductSection/  # CRUD, FluentValidation
│   ├── Commands/ImportProductSections/              # sheet import, preview and apply
│   ├── Queries/GetProductSections/                  # list
│   └── Shared/                                      # fields, type names, specifications
├── Reference.Domain/Reference.Domain/ProductSections/  # entity, details, type, errors, id
└── Reference.Infrastructure/Reference.Infrastructure/
    ├── Persistence/ProductSections/ProductSectionConfiguration.cs
    └── Persistence/Migrations/*_AddProductSections.cs
```

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~ProductSectionTests"`.

**Data:** [reference schema](../../architecture/data-model/reference.md)

## Related Sources

[OpenAPI](../../contracts/reference/product-sections.openapi.yaml) · [Reference boundaries](../../architecture/overview/modules.md).
