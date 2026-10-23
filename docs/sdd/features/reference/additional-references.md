# Additional References

**Static code review:** 2026-10-16. **Module:** reference.

CRUD operations and list retrieval for the “Additional References” lookup.

## Structure and Relationships

[Controller](../../../../src/Modules/Reference/Reference.Api/Reference.Api/Controllers/AdditionalReferencesController.cs) exposes CRUD. The [create handler](../../../../src/Modules/Reference/Reference.Application/Reference.Application/Features/AdditionalReference/Create/CreateAdditionalReferenceCommandHandler.cs) and adjacent query/update/delete handlers validate the data; [AdditionalReference](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/AdditionalReferences/Entities/AdditionalReference.cs) is the domain entity. [ReferenceEfRepository](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Repositories/ReferenceEfRepository.cs) and [ReferenceDbContext](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/DataBase/ReferenceDbContext.cs) persist it. AdditionalReference values are read through the Reference reader during product price calculations.

## Code Structure

Verified entry points and key feature components are shown; the tree does not list every supporting file.

```text
src/
└── Modules/Reference/
    ├── Reference.Api/Reference.Api/Controllers/AdditionalReferencesController.cs  # HTTP entry point
    ├── Reference.Application/Reference.Application/Features/AdditionalReference/
    │   ├── Create/CreateAdditionalReferenceCommandHandler.cs  # use case handler
    │   ├── Delete/DeleteAdditionalReferenceCommandHandler.cs  # use case handler
    │   ├── GetList/GetAdditionalReferenceQueryHandler.cs  # use case handler
    │   └── Update/UpdateAdditionalReferenceCommandHandler.cs  # use case handler
    ├── Reference.Domain/Reference.Domain/AdditionalReferences/Entities/AdditionalReference.cs  # domain model
    └── Reference.Infrastructure/Reference.Infrastructure/
        ├── DataBase/ReferenceDbContext.cs  # data context
        └── Repositories/ReferenceEfRepository.cs  # data access
```

## Structure Diagram

```mermaid
flowchart LR
    A["AdditionalReferencesController"] --> B["CRUD handlers"]
    B --> C["AdditionalReference"]
    B --> D["Reference repository contracts"]
    D -.-> E["ReferenceEfRepository and ReferenceReadEfRepository"]
    E --> F["ReferenceDbContext"]
    E --> C
    G["Product price calculations"] --> H["Reference reader"]
    H --> C
```

## Related Sources

[OpenAPI](../../contracts/reference/additional-references.openapi.yaml) · [Reference boundaries](../../architecture/overview/modules.md).
