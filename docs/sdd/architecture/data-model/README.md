# BreviERP Database Map

**Reviewed:** 2026-10-21 against EF Core `DbContext`, configurations, and snapshots in `src/`. This describes the model structure in the repository; it does not confirm that migrations have been applied in any particular environment. `catalog`, `reference`, `identity`, `accounting`, and `public` are schemas in **one** PostgreSQL database.

```text
BreviERP PostgreSQL
├── catalog / CatalogDbContext — 13 tables
│   ├── Products                       # Sewing and PPE products
│   ├── ProductCategories              # category tree
│   ├── ProductCategoryLinks           # product categories
│   ├── ProductPhotos                  # product photos
│   ├── MediaFiles                     # uploaded file metadata
│   ├── ProductInformationBlocks       # text blocks
│   ├── ProductCharacteristicTables    # characteristic tables
│   ├── ProductCharacteristicRows      # characteristic rows
│   ├── SewingProductDetails           # sewing product details
│   ├── PpeProductDetails              # PPE product details
│   ├── SewingProductFabrics           # product fabrics
│   ├── SewingProductAccessories       # product accessories
│   └── SewingProductOperations        # sewing operations
├── reference / ReferenceDbContext — 6 tables
│   ├── Suppliers                      # suppliers
│   ├── FabricsReference               # fabrics
│   ├── GarmentAccessoriesReference    # garment accessories
│   ├── GarmentParts                   # garment parts
│   ├── GarmentPartOperations          # operations on garment parts
│   └── AdditionalReferences           # additional references
├── identity / AppIdentityDbContext — 8 tables
│   ├── AspNetUsers                    # users
│   ├── AspNetRoles                    # roles
│   ├── AspNetUserRoles                # role assignments
│   ├── AspNetUserClaims               # user claims
│   ├── AspNetRoleClaims               # role claims
│   ├── AspNetUserLogins               # external logins
│   ├── AspNetUserTokens               # tokens
│   └── AppRefreshSessions            # refresh sessions
├── accounting / AccountingDbContext — 1 table
│   └── TransactionCategories          # transaction categories
└── public — operational infrastructure
    └── __EFMigrationsHistory          # EF migration history, per project design
```

## Open Only the Required Module

| Schema and owner | Details |
| --- | --- |
| `catalog` / Catalog | [Catalog tables, PKs, and FKs](catalog.md) |
| `reference` / Reference | [Reference tables, PKs, and FKs](reference.md) |
| `identity` / Identity | [Identity tables, PKs, and FKs](identity.md) |
| `accounting` / Accounting | [TransactionCategories](accounting.md) |

The module documents cover 28 business and Identity model tables; the [module schema decision](../../specs/platform/001-module-schemas/README.md) describes the operational migration history. The Crm module has no tables in the reviewed snapshots. Module documents explain cross-module IDs without physical FKs. After a schema change, update only the affected module and this tree if the table list or placement changed.
