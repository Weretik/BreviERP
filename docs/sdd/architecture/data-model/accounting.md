# `accounting` — Accounting Module Data

**Source:** [Accounting snapshot](../../../../src/Modules/Accounting/Accounting.Infrastructure/Accounting.Infrastructure/Migrations/AccountingDbContextModelSnapshot.cs) and [TransactionCategoryConfiguration](../../../../src/Modules/Accounting/Accounting.Infrastructure/Accounting.Infrastructure/Configurations/TransactionCategoryConfiguration.cs), verified 2026-10-21. [Back to database map](README.md).

`TransactionCategories` has PK `Id` and fields `Name`, `ParentId`, and `Path`. `ParentId` represents the category hierarchy, but the snapshot has no physical FK to the same table. Therefore, the module's ER diagram has no FK relationships.
