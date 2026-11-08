# ACC-001 «Бухгалтерія»

**Status:** stages 1–9 implemented 2026-11-05…08. **Module:** Accounting (schema `accounting`, own migration history). **Consumer:** brevi-web «Бухгалтерія».

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
| D11 | The old migration tools and the Home Accounting runner are not moved; stage 5 is one import of a fresh `brevi-finance-v5.json` with reconciliation — from the admin page «Імпорт» (revised 2026-11-08: the file is uploaded gzipped through the API instead of a `Host.Seed` job, so the director needs no cloud console). The old bank preview and rollback history is not moved (user decision 2026-11-05). |
| D12 | Attachments go to a private GCS bucket (stage 4; user decision 2026-11-05, if it stays within the free tier). |

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | «Рахунки і каси» (create, edit, archive, restore, delete, balance on 01.10) and «Категорії» (two levels, per area and direction), change log, settings | implemented 2026-11-05 |
| 2 | Operations and transfers, account balances, «Огляд», archive view; delete rules: an account with movements is only archived, a category in use is switched off | implemented 2026-11-05 |
| 3 | Counterparties and «Потребують перевірки» | implemented 2026-11-05 |
| 4 | Attachments in a private bucket | implemented 2026-11-08 |
| 5 | Import of the old data (JSON export; the export holds no files) | implemented 2026-11-08 |
| 6 | PrivatBank statement import (XLSX/XLS/CSV) with preview and duplicates | implemented 2026-11-08 (brevi-web «Виписки банку») |
| 7 | «Відкотити виписку»: rollback of a booked statement | implemented 2026-11-08 |
| 8 | Reports, «Журнал змін» tab and JSON export | implemented 2026-11-08 |
| 9 | Links: «Фінанси» in the client and supplier cards, payment of a purchase invoice | implemented 2026-11-08 |

## Stage 1 rules

- Account: name 1–120 characters (trimmed, spaces collapsed), type Bank/Card/Cash/Other, currency three Latin letters (upper-cased, UAH by default), IBAN letters and digits only (upper-cased, at most 64), opening balance in kopecks and its date (2000-01-01…2100-12-31), note at most 600, sort order.
- «Залишок на 01.10»: `confirmed = true` needs an amount and stores who and when; `confirmed = false` clears both.
- «Архівувати» hides the account from the default list; «Показати архівні» lists archived ones last. «Відновити» brings it back.
- «Видалити» removes an unused record; the log keeps its last snapshot. A category root is deleted with its subcategories, each with its own entry. Stage 2 adds the rules for used records below.
- A subcategory must have an active root of the same area and direction; there is no third level.
- Contract: [finance.openapi.yaml](../../../contracts/accounting/finance.openapi.yaml). Feature card: [«Бухгалтерія»](../../../features/accounting/finance.md).

## Stage 2 rules

- An operation is an income or an expense of one account: positive amount in kopecks (`Type` gives the sign), currency from the account, one chosen category (a subcategory, or a root when none is picked), optional one-off name (≤ 250, one line), description (≤ 250) and note (≤ 1500). Source `Manual`; `BankImport` and `HomeAccountingImport` are reserved for stages 5–6 and cannot be edited by the general form.
- The area of a record (`Current` from 2026-10-01, `LegacyArchive` — «Домашня бухгалтерія» — before it) is set by the screen and never changes. A new date must fall into the area; an unchanged date of an existing record is kept. The date is not earlier than the account opening date.
- The current books need an active account and an active category; «Домашня бухгалтерія» accepts an archived account. An account or category the record already had stays allowed after it was archived or switched off. The category must be of the same area and direction.
- A transfer moves money between two different own accounts of one currency; it is neither an income nor an expense. No balance check: an account may go below zero. Note ≤ 1500.
- «Анулювати» makes the record `Void` with a new version and a log entry; nothing is deleted, there is no way back for a hand-made record, and a cancelled record cannot be changed. Cancelled records are shown only with «Показати анульовані» and never count in balances or totals.
- Journals: 50 rows per page (fixed), newest first (ties: the newest saved first, by the time-ordered id); sort by date or amount. Filters: period (inclusive), account (either side for transfers), type, category (a root takes its subcategories) or exact subcategory, text (description, note, one-off name; case-insensitive), amount from/to in kopecks, source, cancelled. Totals cover the whole filtered selection per currency without cancelled rows; transfer totals count each transfer once. Sorting by account, category or counterparty from the old journal is left out.
- Balances (counted on read, never stored): `legacy` = opening balance + active archive movements; `current` = confirmed balance on 01.10.2026 + active current movements, null while not confirmed; `total` = opening balance + every active movement of both areas (not legacy + current). `hasHistory` is true once any movement, cancelled ones too, uses the account.
- «Огляд»: per currency of the active accounts, the balance of the chosen area (both areas when no area is picked) counted from the start of the area and not limited by the period; income, expense and difference of active operations of the period. For the current books the balance sums confirmed accounts only and `unconfirmedCutoverCount` tells the admin to show «Не підтверджено».
- An account with any movement (cancelled ones too) cannot be deleted (409) — only archived; its currency is fixed, and its opening date cannot move past the earliest movement. Deleting a category that any operation uses switches the whole branch off (`Active = false`, log action `Deactivate`) and keeps the ids.
- Log entities `Transaction` and `Transfer`; actions `Void` and `Deactivate`. Snapshots carry the account and category names of that moment.

## Stage 3 rules

- Who an operation is with: a finance counterparty of this module («Фінансові»), a client of «Клієнти» (Crm, by `PublicId`) or a supplier of «Постачальники» (Reference, by id) — stored as `CounterpartyKind` + `CounterpartyId` text without a database link — or a one-off name typed for this operation only, or nobody. A chosen counterparty and a one-off name never go together. The name is never the identity.
- A newly chosen counterparty must be active (archived client, supplier or finance counterparty: 400); the one an operation already has stays allowed. A gone client or supplier gives 404 on save; a row still shows the link with `missing = true`.
- Finance counterparty: name ≤ 250 (one line), ЄДРПОУ/ІПН digits only ≤ 32 (spaces removed), IBAN letters and digits upper-cased ≤ 64, name in the bank ≤ 250, other names (≤ 20, each ≤ 250, repeats and the name itself dropped), default category of the current books (a subcategory or a root, active when newly chosen), auto-apply (for the statement import of stage 6; needs a confirmed counterparty with a default category), note ≤ 600. The form creates it unchecked; «Зберегти як фінансового контрагента» from an operation creates it confirmed.
- Check status: no category → `NeedsCategory` (bank lines, stage 6); an unconfirmed finance counterparty → `NeedsCounterpartyReview`; otherwise `Verified`. Clients and suppliers never need a review. The status is only the quality of the classification: such operations still count in balances, totals and «Огляд».
- «Потребують перевірки» is the current journal with `needsReview`; «Огляд» shows `needsReviewCount` of active current operations whatever the period. «Перевірити» sets only the category and who the operation is with (the date, account and amount stay), needs a confirmed finance counterparty, takes the version (409 when stale) and writes `Review`.
- «Підтвердити» a finance counterparty moves its active operations out of review (to `NeedsCategory` without a category) without a new version of each operation; one `Verify` entry on the counterparty.
- «Зробити постійним» on a one-off name creates a confirmed finance counterparty with that name and links only that operation (`Promote` on the operation, `Create` on the counterparty); other operations with the same name stay as they are.
- «Об'єднати» a duplicate into another active finance counterparty: its operations (cancelled ones too) move to the target, its name and other names become other names of the target, empty codes of the target are filled from it, and it stays archived with `MergedIntoId` and cannot be changed. `Merge` entries on both.
- «Контрагенти» tabs: «Фінансові», «Клієнти», «Постачальники» (read-only here, with the number of active operations), «Разові» (operations with a one-off name, both areas). 50 rows per page; counts of every tab with the same search; «Показати архівні» adds archived records and cancelled operations. Choices of the operation form: active finance counterparties, clients and suppliers matching the search, at most 15 of each kind in that order.
- A category used as a default of a counterparty counts as used: deleting it switches the branch off.
- Crm gives other modules client names through `GetCustomerLookupQuery` (every client regardless of its manager, merged ones never); only «Бухгалтерія» (director) uses it.

## Stage 5 rules

- `POST /api/accounting/legacy-import?dryRun=true|false` takes finance.json (format `brevi-finance-v5`, `complete = true`, `financeCutoverDate = 2026-10-01`), as is or gzipped by the browser; unpacked at most 200 MiB, request at most 32 MiB. A check (`dryRun=true`) writes nothing; the move writes only when the check has no errors, in parts inside one transaction.
- Records keep their old ids, area, source (`HOME_ACCOUNTING` → `HomeAccountingImport`, `BANK_IMPORT` → `BankImport`, `MANUAL` → `Manual`), state, version, author and times. Operations also keep the bank identity (external id, fingerprint, bank id, payment purpose) for the statement import of stage 6. A transfer's personal note is added to its note on a new line. One-off names (none in the current export) are not moved.
- A record whose id Brevi already has is skipped as moved earlier, so a second run adds only what is missing. An account with the name of another Brevi account, or a category with the name of a sibling, under a different id stops the move; so does any record that breaks a rule (the date outside its area, a missing account, category or root). A cancelled record keeps the area it had in the old system.
- Finance counterparties move with their ids and check state, without default categories. Old clients and suppliers used in operations are linked to «Клієнти» by KeepinCRM id (`GetCustomersByKeepinIdsQuery`, archived clients too) and to «Постачальники» by KeepinCRM id or a name only one supplier has; unknown ones become finance counterparties with the old id and a note; unused ones are not moved.
- The five old change logs move for the records added in this run (`CREATE`/`BANK_IMPORT`/`CREATE_FINANCIAL`/`BANK_IMPORT_CREATE` → `Create`, `UPDATE`/`BANK_DETAILS_UPDATE` → `Update`, `VOID`/`ROLLBACK`/`BANK_IMPORT_ROLLBACK` → `Void`, `REPLAY`/`BANK_IMPORT_REPLAY` → `Restore`, `VERIFY` → `Verify`, `CUTOVER_BALANCE_CONFIRM`/`RESET` → `CutoverConfirm`/`CutoverReset`, others → `Update` with a warning). Bank statement previews, links, rollbacks and import batches are not moved.
- The report: per kind in the file, to add, already moved and left out; how clients and suppliers were linked; totals by area, kind and state; per account the balance on 30.09 (opening + archive), the confirmed balance on 01.10 and today, with a warning when the confirmed balance differs from the archive one. Checked on the real export of 2026-11-05: 16 accounts, 492 categories, 41 046 operations, 2 234 transfers, 638 log entries; balances equal the old ones.

## Stage 6 rules

- «Виписки банку» (`/accounting/statements`): the director picks an active account of the current books and a statement file: `.xlsx`, `.xls` or `.csv`, at most 12 MiB, at most 10 000 rows and 128 columns. A password-protected file, a damaged one or one that unpacks to more than 64 MiB is refused. CSV is read as UTF-8 or UTF-16 with a byte order mark, otherwise UTF-8 when valid, otherwise windows-1251; the separator is the one of `,`, `;` and tab that gives the most columns in the first 50 lines.
- The header is the first row among the first 50 of a sheet (the first sheet that has one) with a date column and an amount, debit or credit column. Header names are compared lower-case with punctuation removed and `№`/`#` read as «номер»; the names are those of the old Calculator (`дата проводки`, `сума`, `єдрпоу`, `назва контрагента`, `рахунок контрагента`, `призначення платежу`, `референс` and their variants) plus the card statement (`дата`, `сума в валюті картки`, `опис операції`). Two columns of one kind for the amount, debit, credit or currency stop the file as ambiguous. A card statement has no counterparty column: its lines are matched by the description but never create a counterparty.
- A line: date (`dd.MM.yyyy`, `yyyy-MM-dd`, an Excel date, with or without a time), separate time when there is a time column, otherwise the time of the date cell; credit > 0 → income, debit > 0 → expense, otherwise the sign of the amount (a minus is an expense); the amount is kept positive in kopecks. Empty rows and «Разом/Итого/Залишок/Всього» rows without a date are skipped. A wrong date or amount, a currency other than the account's, a date before 01.10.2026 (the archive) or before the account was opened makes the line an error with the reason; error lines are never added.
- An IBAN in the rows above the header or in the file name that differs from the IBAN of the chosen account stops the file («Виписка іншого рахунку»).
- The same file (same SHA-256) of the same account opens the statement uploaded before instead of a new one.
- Line identity: the bank id (`референс`) and the fingerprint of the old Calculator — SHA-256 of the compact JSON array `[accountId, date, time, amountMinor, currency, type, iban, edrpou, document, bankId, lowercase(trim(purpose))]`, so lines already imported by the old Calculator (moved in stage 5) are recognised. Checked on the 411 readable business-account lines of the old previews: every fingerprint is equal. The old card lines used another formula; they are caught as probable duplicates by the same day, sum and purpose.
- Duplicates. Exact: an operation or transfer of the account (cancelled too) with the same bank id or fingerprint, or an earlier line of the same file with the same fingerprint — never added. Probable: an active operation of the account with the same date, type and amount, and either a hand-made one or one with the same bank purpose; or an active transfer between the same two accounts with the same date and amount. A probable duplicate starts excluded with the reason and can be included by hand.
- Who the line is with, in this order: a finance counterparty by ЄДРПОУ, a client of «Клієнти» by ЄДРПОУ (`GetCustomersByTaxIdsQuery`), a supplier by ЄДРПОУ, a finance counterparty by IBAN, then by its name, bank name or another name equal to the bank name, or to the description when the line has no bank name (ignoring case and repeated spaces). Only active records count; finance counterparties merged into others never do. An IBAN of another active own account in the same currency makes the line a transfer instead.
- The category: the default category of the matched finance counterparty when it has auto-apply. Line states, in this order: error; exact or probable duplicate; transfer; new counterparty (none matched, or an unconfirmed finance counterparty); needs category; ready. The director can change a line before the statement is booked: category of the current books in the direction of the line, counterparty or a one-off name, description, note, include or exclude; transfer lines only take a note. Date, amount, account and the bank data stay protected.
- «Провести виписку» books every included line of states ready, needs category and new counterparty, plus transfers, in one database transaction, after checking exact duplicates again: operations with source `BankImport`, bank id as the external id, the fingerprint, the bank id and purpose, check status from the category and the counterparty (no category → «Потребують перевірки»); transfers between the two accounts (an expense line goes from the chosen account, an income line comes into it) with the purpose and the note as the note. A line without a counterparty that has a bank name, ЄДРПОУ or IBAN creates one unconfirmed finance counterparty per ЄДРПОУ, else IBAN, else name in the statement. `Create` log entries for each record. The statement becomes booked with the numbers of operations and transfers and remembers the record of each line for the rollback of stage 7.
- «Історія виписок»: the last 100 statements with the account, file, who uploaded it and when, the range of line dates, the number of lines, the state and the booked numbers.

## Stage 7 rules

- «Відкотити виписку» on a booked statement (`POST /api/accounting/statements/{id}/rollback` with the version the page loaded) asks first and shows what it will do: the active operations and transfers of the statement, how many of them were changed by hand after the booking (version above 1) and how many are cancelled already (`rollback` of the statement details).
- In one save: every active operation and transfer the statement booked is cancelled (Void, kept in the journal, one «Анулювання» entry each in the change log) and gives up its bank identity (bank id, external id, fingerprint), so the lines are no duplicates when the statement is booked again. A record the director cancelled by hand before keeps its identity and the line is again an exact duplicate.
- An unconfirmed finance counterparty the booking created (`CreatedCounterparty` of the line) is archived when no active operation outside the statement uses it; a confirmed, merged or used one stays and its lines keep it. Lines of an archived one wait for a counterparty again.
- The statement becomes a preview: no booked numbers, who and when booked cleared, lines unlinked from their records, version + 1. Correct it and book it again, or leave it.
- The old Calculator's "restore from a JSON backup" is not ported: the archive move of stage 5 and these rollbacks cover it.

## Stage 8 rules

- «Звіти» (`GET /api/accounting/reports`) builds one of six reports for an inclusive period, an area (or both), and optionally an account, a type, a category branch and one counterparty: «Зведений», «Структура витрат», «За категоріями», «За рахунками», «За контрагентами», «Журнал операцій». Only active income and expense count; totals are per currency, with no conversion, and never include transfers.
- «Структура витрат» is always expenses: a root category row holds its subcategories, which follow it; direct spending on a root that also has subcategories is «Без підкатегорії». The rows must not be added up together.
- «За рахунками» shows the movement of the period, not the balance: income, expense, transfers in and out, and the net movement. Transfers are filtered only by the period, area and account.
- «За контрагентами» names the counterparty (finance, client or supplier); a one-off name is marked «(разовий)», no counterparty is «Без контрагента», and a link to a removed record is «Контрагента не знайдено».
- «Журнал операцій» lists the operations oldest first: 100 on the screen, up to 50 000 in the CSV file. A longer period is split by dates.
- «Завантажити CSV» (`GET /api/accounting/reports/csv`) gives the same report for Excel: UTF-8 with BOM, «;», CRLF, every field quoted, sums in hryvnias with a decimal comma, dates dd.MM.yyyy. Text that Excel would read as a formula gets a leading apostrophe.
- «Журнал змін» (`GET /api/accounting/audit`) lists the changes of all records, newest first, 50 per page, optionally of one kind of record or one action. Each row names the record from its snapshot (a name, or date, sum and description) and opens what changed.
- «Резервна копія» (`GET /api/accounting/export`) downloads every stored field of every finance record in one JSON file (`brevi-finance-YYYY-MM-DD.json`, format `brevi-erp-finance-v1`), cancelled records, the change log and bank statements with their lines included. Files of operations are not in it; they stay in the private bucket. It is a copy for safekeeping; restoring from it is not ported (stage 7 rules).

## Stage 9 rules

- «Фінанси» in the client card and in the viewed supplier card (brevi-web) lists the active income and expense with that counterparty through `GET /api/accounting/transactions?counterpartyKind=&counterpartyId=` (client: public id, supplier: number): 50 per page, newest first, totals per currency (zero hryvnias when empty), «Поточний облік» / «Домашня бухгалтерія» tabs, an optional period «З»/«По» applied on «Показати». Transfers are not here. A row opens the operation panel of «Операції». The tab needs the «Бухгалтерія» right (`access:accounting`); no new page or key.
- Payment of a purchase invoice (user decision 2026-11-08: manual link with a hint) is kept in Purchasing (`purchasing.Payments`, [PUR-001](../../purchasing/001-purchase-invoices/README.md#payment-links-accounting-stage-9)); nothing is booked in «Бухгалтерія». Accounting answers two in-process queries for the host: `GetFinanceTransactionsByIdsQuery` (operations by id, voided too) and `GetFinancePaymentCandidatesQuery` (active expenses of both areas: of a supplier, or by text or amount in hryvnias, or equal to the invoice total; that amount first, then the newest; at most 20).
