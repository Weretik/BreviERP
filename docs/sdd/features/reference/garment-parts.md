# Garment Parts

**Static code review:** 2026-10-16. **Module:** reference.

CRUD operations and list retrieval for the “Garment Parts” lookup.

## Structure and Relationships

[Controller](../../../../src/Modules/Reference/Reference.Api/Reference.Api/GarmentPart/GarmentPartsController.cs) exposes CRUD. The [create handler](../../../../src/Modules/Reference/Reference.Application/Reference.Application/Features/GarmentPart/Commands/CreateGarmentPart/CreateGarmentPartHandler.cs) and adjacent query/update/delete handlers validate the data; [GarmentPart](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/GarmentPartOperations/GarmentPart.cs) is the domain entity. [ReferenceEfRepository](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Persistence/ReferenceEfRepository.cs) and [ReferenceDbContext](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Persistence/ReferenceDbContext.cs) persist it. A garment part groups sewing operations.

## Code Structure

Verified entry points and key feature components are shown; the tree does not list every supporting file.

```text
src/
└── Modules/Reference/
    ├── Reference.Api/Reference.Api/GarmentPart/GarmentPartsController.cs  # HTTP entry point
    ├── Reference.Application/Reference.Application/Features/GarmentPart/
    │   ├── Commands/CreateGarmentPart/CreateGarmentPartHandler.cs  # use case handler
    │   ├── Commands/DeleteGarmentPart/DeleteGarmentPartHandler.cs  # use case handler
    │   ├── Queries/GetGarmentParts/GetGarmentPartsHandler.cs  # use case handler
    │   └── Commands/UpdateGarmentPart/UpdateGarmentPartHandler.cs  # use case handler
    ├── Reference.Domain/Reference.Domain/GarmentPartOperations/GarmentPart.cs  # domain model
    └── Reference.Infrastructure/Reference.Infrastructure/
        ├── Persistence/ReferenceDbContext.cs  # data context
        └── Persistence/ReferenceEfRepository.cs  # data access
```

## Structure Diagram

```mermaid
flowchart LR
    A["GarmentPartsController"] --> B["CRUD handlers"]
    B --> C["GarmentPart"]
    B --> D["Reference repository contracts"]
    D -.-> E["ReferenceEfRepository and ReferenceReadEfRepository"]
    E --> F["ReferenceDbContext"]
    E --> C
    C --> G["Sewing operations"]
```

## Tests

No automated tests cover this feature's CRUD operations yet. `tests/UnitTests/Reference/ReferenceProductUsageTests.cs` covers rejecting deletion of reference data used by a Product.

**Data:** [reference schema](../../architecture/data-model/reference.md)

## Related Sources

[OpenAPI](../../contracts/reference/garment-parts.openapi.yaml) · [Reference boundaries](../../architecture/overview/modules.md).
