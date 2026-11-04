# BreviERP Database Map

**Reviewed:** 2026-10-21 against EF Core `DbContext`, configurations, and snapshots in `src/`. This describes the model structure in the repository; it does not confirm that migrations have been applied in any particular environment. `catalog`, `reference`, `identity`, `accounting`, and `public` are schemas in **one** PostgreSQL database.

```text
BreviERP PostgreSQL
├── catalog / CatalogDbContext — 18 tables
│   ├── Products                       # Sewing and PPE products
│   ├── ProductCatalogProfiles         # activity/season/source review and PPE offers
│   ├── SewingPricingSettingsVersions # immutable effective pricing settings and source evidence
│   ├── SewingArticles                # sale SKUs per sewing model
│   ├── SewingMaterialLines           # shared and Article-specific material norms
│   ├── SewingOperationLines          # individual shared operation occurrences
│   ├── ProductCategories              # category tree
│   ├── ProductCategoryLinks           # product categories
│   ├── ProductPhotos                  # product photos
│   ├── MediaFiles                     # legacy copy, no longer mapped; photos live in media.Files
│   ├── ProductInformationBlocks       # text blocks
│   ├── ProductCharacteristicTables    # characteristic tables
│   ├── ProductCharacteristicRows      # characteristic rows
│   ├── SewingProductDetails           # sewing product details
│   ├── PpeProductDetails              # PPE product details
│   ├── PpePriceSources                # «Імпорт цін» discount and supplier per source
│   ├── PpePriceImportRuns             # «Імпорт цін» applied runs with problems
│   ├── SewingProductFabrics           # product fabrics
│   ├── SewingProductAccessories       # product accessories
│   └── SewingProductOperations        # sewing operations
├── reference / ReferenceDbContext — 7 tables
│   ├── Suppliers                      # suppliers
│   ├── PricingInputVersions           # effective material/OtherCost costs and operation norms
│   ├── FabricsReference               # fabrics
│   ├── GarmentAccessoriesReference    # garment accessories
│   ├── GarmentParts                   # garment parts
│   ├── GarmentPartOperations          # operations on garment parts
│   ├── PricingParameters              # «Параметри ціноутворення»: sheet «Налаштування цін», left table
│   └── PpeMarkupRules                 # PPE markup rules: right table of the same sheet
├── identity / AppIdentityDbContext — 8 tables
│   ├── AspNetUsers                    # users
│   ├── AspNetRoles                    # roles
│   ├── AspNetUserRoles                # role assignments
│   ├── AspNetUserClaims               # user claims
│   ├── AspNetRoleClaims               # role claims
│   ├── AspNetUserLogins               # external logins
│   ├── AspNetUserTokens               # tokens
│   └── DataProtectionKeys            # keys that sign the session cookie
├── accounting / AccountingDbContext — 1 table
│   └── TransactionCategories          # transaction categories
├── media / MediaDbContext — 2 tables + own history
│   ├── Folders                        # photo folder tree
│   └── Files                          # photos on the CDN
├── production / ProductionDbContext — 5 tables + own history
│   ├── Employees                      # workers of the workshop
│   ├── FabricNorms                    # fabric per item of a sewing model
│   ├── Orders                         # «Виробничі завдання» from КП
│   ├── Items                          # positions of an order with their snapshot
│   └── OrderChanges                   # «Історія змін» of an order
├── reminders / RemindersDbContext — 6 tables + own history
│   ├── Tasks                          # reminder tasks and schedule
│   ├── Occurrences                    # one period per task: deadline, status, who closed
│   ├── OccurrenceActions              # done / postponed / reopened journal
│   ├── Chats                          # Telegram chats and approval
│   ├── SentMessages                   # cards sent per period and day
│   └── NonWorkingDays                 # extra days off
├── imports / CatalogImportDbContext — 2 job-owned tables
│   ├── CatalogImportBatches           # snapshot, report, checksum
│   └── CatalogSourceMappings          # source-to-Brevi identity mapping
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

The module documents cover 28 business and Identity model tables; the [module schema decision](../../specs/platform/001-module-schemas/README.md) describes the operational migration history. Schema `crm` holds five client tables and six КП tables; see [Crm](crm.md). The earlier counts/tree above are the historical 2026-10-21 snapshot, not a current total. Module documents explain cross-module IDs without physical FKs. After a schema change, update only the affected module and this tree if the table list or placement changed.
