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
├── identity / AppIdentityDbContext — 9 tables
│   ├── AspNetUsers                    # users
│   ├── AspNetRoles                    # roles
│   ├── AspNetUserRoles                # role assignments
│   ├── AspNetUserClaims               # user claims
│   ├── AspNetRoleClaims               # role claims
│   ├── AspNetUserLogins               # external logins
│   ├── AspNetUserTokens               # tokens
│   ├── DataProtectionKeys            # keys that sign the session cookie
│   └── role_access                   # «Права доступу»: saved rights per role
├── accounting / AccountingDbContext — 9 tables + own history
│   ├── Accounts                       # «Рахунки і каси» with the balance on 01.10
│   ├── Categories                     # «Категорії»: two levels per area and direction
│   ├── Transactions                   # «Операції» with the counterparty kind and id
│   ├── Transfers                      # «Перекази» between own accounts
│   ├── Counterparties                 # finance counterparties («Фінансові»)
│   ├── Attachments                    # files of operations in the private bucket
│   ├── BankStatements                 # «Виписки банку»: uploaded statements
│   ├── BankStatementLines             # their lines with what was recognised and chosen
│   └── AuditEntries                   # change log with JSON snapshots
├── media / MediaDbContext — 2 tables + own history
│   ├── Folders                        # photo folder tree
│   └── Files                          # photos on the CDN
├── production / ProductionDbContext — 18 tables + own history
│   ├── Employees                      # workers of the workshop
│   ├── FabricNorms                    # fabric per item of a sewing model
│   ├── Orders                         # «Виробничі завдання» from КП
│   ├── Items                          # positions of an order with their snapshot
│   ├── OrderChanges                   # «Історія змін» of an order
│   ├── Work                           # «Журнал робіт»: pieces sewn or cut
│   ├── ForecastQueues                 # saved order of «Прогноз пошиття»
│   ├── TimeEntries, TimeChanges       # «Робочий час» and its history
│   ├── PayrollMonths, PayrollRoles    # «Зарплата»: month, embroidery, closing; master and chief
│   └── PayrollPayments, PayrollAdjustments, PayrollIssued  # payments, adjustments, «Видано»
├── ai / AiDbContext — 3 tables + own history
│   ├── Settings                       # «Налаштування ШІ»: encrypted OpenAI key, models, prices, limit
│   ├── UsageMonths                    # requests, tokens and cost per month
│   └── Prompts                        # the admin's extra rules per request kind
├── purchasing / PurchasingDbContext — 9 tables + own history
│   ├── Messages                       # «Рахунки на закупку»: one Telegram message
│   ├── SourceRows                     # one UID of the sheet «финансы»
│   ├── SourceRevisions                # raw history of a row
│   ├── MessageFiles                   # links to original invoices
│   ├── ReviewChanges                  # history of the supplier link and note
│   ├── CheckedDocuments               # checked facts of one original
│   ├── CheckedDocumentHistory         # replaced facts
│   ├── RecognitionAttempts            # each «Розпізнати» with its problems
│   └── SyncStates                     # last sheet sync
├── warehouse / WarehouseDbContext — 4 tables + own history
│   ├── FabricStocks                   # «Склад тканин»: fabric + colour balance
│   ├── FabricMovements                # every change of a balance
│   ├── FabricIssues                   # «Видача» packages to order items
│   └── FabricIssueLines               # lines of a package with plan coverage
├── content / ContentDbContext — 10 tables + own history
│   ├── ProductPages                   # site page of a Catalog product: mark, address, revision
│   ├── ProductPageTexts               # its texts in one language (uk, ru)
│   ├── SiteCategories                 # category tree of the site (two levels)
│   ├── SiteCategoryTexts              # name, SEO and text of a category in one language
│   ├── SiteCategoryProducts           # products of a category
│   ├── Articles                       # site article: address, date, mark, cover photo id
│   ├── ArticleTexts                   # title, excerpt, SEO and body of an article in one language
│   ├── SiteRedirects                  # 301 redirects of the site: old address → new page
│   ├── SiteSeoSettings                # one row: analytics ids, verification codes, sharing picture
│   ├── SitePages                      # search texts, main text and FAQ of a fixed site page (uk, ru)
│   ├── SiteCompany                    # one row: company phone, email, address, hours, managers, bank details, links
│   └── SiteGalleries                  # rows works and certificates: site pictures in order with their texts
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
| `accounting` / Accounting | [«Бухгалтерія» accounts, categories, operations, transfers, counterparties, change log](accounting.md) |
| `ai` / Ai | [AI settings](ai.md) |
| `purchasing` / Purchasing | [Purchase invoice register](purchasing.md) |
| `content` / Content | [Site pages of products, site categories, articles, redirects, SEO settings](content.md) |
| `warehouse` / Warehouse | [«Склад тканин»: lines, movements, issues](warehouse.md) |

The module documents cover 28 business and Identity model tables; the [module schema decision](../../specs/platform/001-module-schemas/README.md) describes the operational migration history. Schema `crm` holds five client tables and six КП tables; see [Crm](crm.md). The earlier counts/tree above are the historical 2026-10-21 snapshot, not a current total. Module documents explain cross-module IDs without physical FKs. After a schema change, update only the affected module and this tree if the table list or placement changed.
