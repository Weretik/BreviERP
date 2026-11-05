# ACC-001 «Бухгалтерія»

**Status:** stage 1 implemented 2026-11-05. **Module:** Accounting (schema `accounting`, own migration history). **Consumer:** brevi-web «Бухгалтерія».

## Goal

Move the «Фінанси» section of the old Calculator (version 217) into Brevi as the «Бухгалтерія» tab: accounts and cash desks, categories, operations and transfers, counterparties, attachments, PrivatBank statement import, reports and a change log. The client's technical description of version 217 is kept outside the repository (project files); this SDD records only the decisions for Brevi. The full plan with the user's answers is the Claude Doc «Бухгалтерія Brevi: аналіз ТЗ і план етапів».

## Decisions

| ID | Decision |
| --- | --- |
| D1 | The empty Accounting stub (Wallet/Transaction/TransactionCategory, no API, history in `public`) is replaced. The module keeps schema `accounting`, moves its history to `accounting.__EFMigrationsHistory` with a new `InitialAccounting` migration and drops the unused stub table `TransactionCategories`. |
| D2 | Access only for the Director: policy `CanUseAccounting` = role Director (user decision 2026-11-05). Admin, Accountant and Manager get 403. |
| D3 | Ids are `Guid` v7 (Vogen `FinanceAccountId`, `FinanceCategoryId`): the old export brings its own ids, and stage 5 imports them unchanged. |
| D4 | Money is a whole number of kopecks (`long`, at most 99 999 999 999 999). All accounts are in UAH now; the currency field stays for other banks later (user decision 2026-11-05). |
| D5 | Two areas of the books: `LegacyArchive` («Домашня бухгалтерія», dates before 2026-10-01) and `Current` (from 2026-10-01). «Today» is the date in Europe/Kyiv. |
| D6 | Each account has the confirmed balance on 01.10.2026 (`CutoverBalanceMinor`, who and when). An account without it shows only the problem «Не підтверджено». |
| D7 | Categories have two levels inside one area and direction (Income/Expense). Names are unique among siblings ignoring case. Default categories come from the old export (stage 5), not from code (user decision 2026-11-05). |
| D8 | One change log table `AuditEntries` for all records: entity, id, action, who (e-mail), when, JSON snapshots before and after. |
| D9 | Every change sends the `Version` the user opened; a stale version → 409 «Запис уже змінили…». Saving the same values changes nothing. |
| D10 | Clients come from Crm and suppliers from Reference by id without a database link; only finance counterparties are owned here (stage 3). |
| D11 | The old migration tools and the Home Accounting runner are not moved; stage 5 is one `Host.Seed` import of a fresh `brevi-finance-v5.json` with reconciliation. The old bank preview and rollback history is not moved (user decision 2026-11-05). |
| D12 | Attachments go to a private GCS bucket (stage 4; user decision 2026-11-05, if it stays within the free tier). |

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | «Рахунки і каси» (create, edit, archive, restore, delete, balance on 01.10) and «Категорії» (two levels, per area and direction), change log, settings | implemented 2026-11-05 |
| 2 | Operations and transfers, account balances, «Огляд», archive view; delete rules: an account with movements is only archived, a category in use is switched off | planned |
| 3 | Counterparties and «Потребують перевірки» | planned |
| 4 | Attachments in a private bucket | planned |
| 5 | Import of the old data (JSON export + attachments ZIP) | planned |
| 6 | PrivatBank statement import (XLSX/CSV) with preview and duplicates | planned |
| 7 | Import rollback and restore | planned |
| 8 | Reports, «Журнал змін» tab and JSON export | planned |
| 9 | Links: «Фінанси» in the counterparty card, invoice payment | later |

## Stage 1 rules

- Account: name 1–120 characters (trimmed, spaces collapsed), type Bank/Card/Cash/Other, currency three Latin letters (upper-cased, UAH by default), IBAN letters and digits only (upper-cased, at most 64), opening balance in kopecks and its date (2000-01-01…2100-12-31), note at most 600, sort order.
- «Залишок на 01.10»: `confirmed = true` needs an amount and stores who and when; `confirmed = false` clears both.
- «Архівувати» hides the account from the default list; «Показати архівні» lists archived ones last. «Відновити» brings it back.
- «Видалити» in stage 1 removes the record; the log keeps its last snapshot. A category root is deleted with its subcategories, each with its own entry.
- A subcategory must have an active root of the same area and direction; there is no third level.
- Contract: [finance.openapi.yaml](../../../contracts/accounting/finance.openapi.yaml). Feature card: [Finance accounts and categories](../../../features/accounting/finance.md).
