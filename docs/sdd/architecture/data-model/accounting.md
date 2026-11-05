# Accounting schema

Code model reviewed 2026-11-05. `Accounting.Infrastructure.Persistence.AccountingDbContext` owns schema `accounting`; migration history is `accounting.__EFMigrationsHistory`. Migration `InitialAccounting` creates the stage 1 tables and drops the unused stub table `TransactionCategories` of the earlier `InitialAccountingSchema` (whose history row stays in `public`) ([ACC-001](../../specs/accounting/001-finance/README.md)). [Back to database map](README.md).

| Table | Identity / relationships / purpose |
| --- | --- |
| Accounts | PK Id (uuid v7). Name, Type (text), Currency (char 3), Iban, OpeningMinor (bigint, kopecks), OpeningDate, CutoverBalanceMinor, CutoverConfirmed, CutoverConfirmedAt, CutoverConfirmedBy, Note, Active, SortOrder, Version (concurrency token), CreatedAt, UpdatedAt; index (Active, SortOrder) |
| Categories | PK Id (uuid v7); ParentId FK → Categories (restrict), two levels. Name, Direction (Income/Expense), Scope (Current/LegacyArchive), Note, Active, SortOrder, Version (concurrency token), CreatedAt, UpdatedAt; index (Scope, Direction, ParentId) |
| AuditEntries | PK Id (identity). Entity (Account/Category), EntityId, Action, By (e-mail), At, BeforeJson, AfterJson (jsonb snapshots); index (Entity, EntityId, Id), index At. Append-only change log |

No links to other modules yet; stage 3 stores Crm client and Reference supplier ids without database links.
