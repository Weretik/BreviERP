# `reference` — Reference Module Data

**Source:** [Reference snapshot](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Migrations/ReferenceDbContextModelSnapshot.cs) and [configurations](../../../../src/Modules/Reference/Reference.Infrastructure/Reference.Infrastructure/Congigurations/), verified 2026-10-21. [Back to database map](README.md).

| Table | PK | Main data and purpose |
| --- | --- | --- |
| `Suppliers` | `Id` | Suppliers and contacts. |
| `FabricsReference` | `Id` | Fabrics: `Name`, `Price`, `ProviderId` (FK to `Suppliers`). |
| `GarmentAccessoriesReference` | `Id` | Accessories: `Name`, `Price`, `SupplierId` (FK). |
| `GarmentParts` | `Id` | Garment parts. |
| `GarmentPartOperations` | `Id` | Operations: `GarmentPartId`, `Name`, `Min`. |
| `AdditionalReferences` | `Id` | Additional reference values: `Key`, `Name`, `Value`, `Unit`. |

```mermaid
erDiagram
    Suppliers ||--o{ FabricsReference : "ProviderId FK"
    Suppliers ||--o{ GarmentAccessoriesReference : "SupplierId FK"
```

`GarmentPartOperations.GarmentPartId` refers conceptually to `GarmentParts`, but the snapshot has no FK. The module's tables are described in [suppliers](../../features/reference/suppliers.md), [fabrics](../../features/reference/fabrics.md), [accessories](../../features/reference/garment-accessories.md), [garment parts](../../features/reference/garment-parts.md), [operations](../../features/reference/garment-part-operations.md), and [additional references](../../features/reference/additional-references.md).
