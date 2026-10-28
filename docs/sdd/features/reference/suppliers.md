# Suppliers

**Static code review:** 2026-10-16. **Module:** reference.

CRUD operations and list retrieval for the “Suppliers” lookup.

## Structure and Relationships

[Controller](../../../../src/Modules/Reference/Reference.Api/Reference.Api/Controllers/SuppliersController.cs) exposes CRUD. The [create handler](../../../../src/Modules/Reference/Reference.Application/Reference.Application/Features/Supplier/Create/CreateSupplierCommandHandler.cs) and adjacent query/update/delete handlers validate the data; [Supplier](../../../../src/Modules/Reference/Reference.Domain/Reference.Domain/Suppliers/Entities/Supplier.cs) is the domain entity. [ReferenceEfRepository](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Repositories/ReferenceEfRepository.cs) and [ReferenceDbContext](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/DataBase/ReferenceDbContext.cs) persist it. Suppliers are external reference data for PPE products and garment accessories.

## Code Structure

Verified entry points and key feature components are shown; the tree does not list every supporting file.

```text
src/
└── Modules/Reference/
    ├── Reference.Api/Reference.Api/Controllers/SuppliersController.cs  # HTTP entry point
    ├── Reference.Application/Reference.Application/Features/Supplier/
    │   ├── Create/CreateSupplierCommandHandler.cs  # use case handler
    │   ├── Delete/DeleteSupplierCommandHandler.cs  # use case handler
    │   ├── GetList/GetSuppliersQueryHandler.cs  # use case handler
    │   └── Update/UpdateSupplierCommandHandler.cs  # use case handler
    ├── Reference.Domain/Reference.Domain/Suppliers/Entities/Supplier.cs  # domain model
    └── Reference.Infrastructure/Reference.Infrastructure/
        ├── DataBase/ReferenceDbContext.cs  # data context
        └── Repositories/ReferenceEfRepository.cs  # data access
```

## Structure Diagram

```mermaid
flowchart LR
    A["SuppliersController"] --> B["CRUD handlers"]
    B --> C["Supplier"]
    B --> D["Reference repository contracts"]
    D -.-> E["ReferenceEfRepository and ReferenceReadEfRepository"]
    E --> F["ReferenceDbContext"]
    E --> C
    G["PPE product"] --> C
    H["Garment accessory"] --> C
```

## Tests

No automated tests cover this feature's CRUD operations yet. `tests/UnitTests/ReferenceProductUsageTests.cs` covers rejecting deletion of reference data used by a Product.

**Data:** [reference schema](../../architecture/data-model/reference.md)

## Related Sources

[OpenAPI](../../contracts/reference/suppliers.openapi.yaml) · [Reference boundaries](../../architecture/overview/modules.md).
