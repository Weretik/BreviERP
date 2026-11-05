# PUR-001 «Рахунки на закупку»

**Status:** stage 1 backend implemented 2026-11-05. **Module:** Purchasing (new, schema `purchasing`). **Consumer:** brevi-web «Загальні довідники → Контрагенти → Рахунки на закупку».

## Goal

Move the purchase invoice register of the old Calculator (section «Контрагенти та рахунки», version 217) into Brevi. The source is the Google Sheet «БРЕВІ команда», sheet «финансы» (A:N), filled by the Telegram bot: one row per message part, with Dropbox links to the original invoices and the payment mark «СПЛАЧЕНО». Brevi only reads the sheet.

The client's technical description of version 217 is kept outside the repository (project files); this SDD records only the decisions for Brevi.

## Decisions

| ID | Decision |
| --- | --- |
| D1 | New module Purchasing with its own schema and migration history. The supplier is stored as a Reference supplier id without a database link across modules. |
| D2 | Same deterministic ids as the Calculator (SHA-256 of `telegram:<ChatId>:<MessageId>`, `<spreadsheetId>:<sheetId>:<UID>`, `<messageId>:<url>`, `<sourceId>:<rawJson>`), and `rawJson` written like JavaScript `JSON.stringify`. Checked against the old database export on 2026-11-05: 740 of 740 rows give the same ids and raw text, so a fresh sync recreates the same records. |
| D3 | One server-side sync instead of the Calculator's three-step plan/chunk/finish (needed there only for Cloudflare limits): read and check the whole sheet, then apply it in one transaction under a PostgreSQL advisory lock. A failed or parallel run changes nothing (fixes the old SI-05/SI-14 gaps). |
| D4 | The sheet is read with the Cloud Run runtime service account (Application Default Credentials, scope `spreadsheets.readonly`); the sheet is shared with that account as a viewer. No key file, no owner OAuth token. |
| D5 | Access by role, not by one e-mail: policy `CanUsePurchaseInvoices` = Accountant and Admin (the Director also holds Admin). Managers get 403 (user decision 2026-11-05). |
| D6 | The sync never changes the manual supplier link, note and their version. Rows and links missing from the sheet become inactive and stay. |
| D7 | No «all good» status: the card shows only problems with their reason (`NeedsReview` + issues). |
| D8 | The «Фінанси» tab of version 217 is not moved: Brevi has no finance module. |
| D9 | Old data: the export has no supplier links, notes or audit and one checked document; that document is moved in stage 3. No separate migration stage. |

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | Register: tables, sheet read, sync («Оновити зараз» + daily step `purchase-invoices`), list with search and «Потребує перевірки» filter | backend implemented 2026-11-05 |
| 2 | Manual supplier link and note with optimistic version and audit | planned |
| 3 | Checked invoice JSON: validation of VAT arithmetic in kopecks, hash, idempotent save; move the one old checked document | planned |
| 4 | Read-only «Рахунки на закупку» in the supplier card; «КП» and «Завдання в цех» in the client card | planned |

## Stage 1 rules

- Header A1:N1 must be exactly `UID, ChatName, ChatId, MessageId, DateTime, FromUser, FromUserId, Text, MessageType, FileLinks, FileCount, Status, StatusDate, FromUserStatus`; the sheet id and title must match; the grid has 1–20 000 rows.
- Fully empty rows are skipped; a non-empty row without UID, ChatId or MessageId, a repeated UID, different payment marks within one message, or a UID that moved to another Telegram message stops the sync (422) and is recorded as the last error.
- Problems kept on the row (the row stays in the register): FileCount empty, not a whole number or different from the number of distinct links; a link that is not HTTPS Dropbox or has a user/password; FileLinks text without links; no original link.
- `MarkedPaid` only when trimmed, upper-cased Status equals «СПЛАЧЕНО». It is a note, not a payment.
- One card per message; the representative row is the active row with the smallest row number. Order: recognised message date (DD.MM.YYYY, DD/MM/YYYY, ISO) newest first, unknown dates last, then UID.

## Contract and tests

[OpenAPI](../../../contracts/purchasing/purchase-invoices.openapi.yaml): `getPurchaseInvoices`, `getPurchaseInvoicesSyncState`, `syncPurchaseInvoices`. Tests: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Purchasing"` (source rules, sync on SQLite, list query, contract) and `tests/ArchitectureTests/PurchasingDependencyTests.cs`.

## Deployment

The migration `InitialPurchasing` runs with the normal deploy. Before the first sync: enable Google Sheets API in project `brevistoreapi` and share the sheet with the runtime service account as a viewer ([runbook](../../../operations/jobs/sync-job.md#purchase-invoices)).
