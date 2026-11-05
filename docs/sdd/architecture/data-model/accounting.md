# Accounting schema

Code model reviewed 2026-11-05. `Accounting.Infrastructure.Persistence.AccountingDbContext` owns schema `accounting`; migration history is `accounting.__EFMigrationsHistory`. Migration `InitialAccounting` creates the stage 1 tables and drops the unused stub table `TransactionCategories` of the earlier `InitialAccountingSchema` (whose history row stays in `public`); `AddFinanceMovements` adds operations and transfers ([ACC-001](../../specs/accounting/001-finance/README.md)). [Back to database map](README.md).

| Table | Identity / relationships / purpose |
| --- | --- |
| Accounts | PK Id (uuid v7). Name, Type (text), Currency (char 3), Iban, OpeningMinor (bigint, kopecks), OpeningDate, CutoverBalanceMinor, CutoverConfirmed, CutoverConfirmedAt, CutoverConfirmedBy, Note, Active, SortOrder, Version (concurrency token), CreatedAt, UpdatedAt; index (Active, SortOrder) |
| Categories | PK Id (uuid v7); ParentId FK → Categories (restrict), two levels. Name, Direction (Income/Expense), Scope (Current/LegacyArchive), Note, Active, SortOrder, Version (concurrency token), CreatedAt, UpdatedAt; index (Scope, Direction, ParentId) |
| Transactions | PK Id (uuid v7); AccountId FK → Accounts (restrict), CategoryId FK → Categories (restrict, null only for future bank lines). Date, Type (Income/Expense), AmountMinor (bigint > 0), Currency (char 3, from the account), OneoffName, Description, Comment, Source, Verification, Scope, Status (Active/Void), Version (concurrency token), CreatedBy, CreatedAt, UpdatedAt; indexes (Scope, Status, Date, Id), (AccountId, Status, Scope, Date), CategoryId |
| Transfers | PK Id (uuid v7); FromAccountId and ToAccountId FK → Accounts (restrict), check: different accounts, AmountMinor > 0. Date, Currency, Note, Source, Scope, Status, Version, CreatedBy, CreatedAt, UpdatedAt; indexes (Scope, Status, Date, Id), FromAccountId, ToAccountId |
| AuditEntries | PK Id (identity). Entity (Account/Category/Transaction/Transfer), EntityId, Action, By (e-mail), At, BeforeJson, AfterJson (jsonb snapshots); index (Entity, EntityId, Id), index At. Append-only change log |

Balances are counted on read and not stored. No links to other modules yet; stage 3 stores Crm client and Reference supplier ids without database links.
