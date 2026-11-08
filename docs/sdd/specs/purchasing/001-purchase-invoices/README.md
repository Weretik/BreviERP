# PUR-001 «Рахунки на закупку»

**Status:** stages 1–3 implemented 2026-11-05. **Module:** Purchasing (new, schema `purchasing`). **Consumer:** brevi-web «Загальні довідники → Контрагенти → Рахунки на закупку».

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
| D5 | Access by role, not by one e-mail: policy `CanUsePurchaseInvoices` = Director only (client corrections item 10, 2026-11-08; before that Accountant and Admin). Others get 403; the admin page moved to «Бухгалтерія → Рахунки на закупку». |
| D6 | The sync never changes the manual supplier link, note and their version. Rows and links missing from the sheet become inactive and stay. |
| D7 | No «all good» status: the card shows only problems with their reason (`NeedsReview` + issues). |
| D8 | The «Фінанси» tab of version 217 is not moved: Brevi has no finance module. |
| D9 | Old data: the export has no supplier links, notes or audit and one checked document; that document is moved in stage 3 by uploading its facts as JSON (converted to the stage 3 format). No separate migration stage. |
| D10 | Stage 3 reads originals with OpenAI through the Ai module ([AI-001](../../ai/001-ai-settings/README.md); user decision 2026-11-05, replacing the earlier Gemini choice). Only the facts the accountant needs (user decision 2026-11-05): document type, number, date, supplier, VAT mode and positions as printed, VAT amount and total. The old 217 schema with per-line VAT columns is not kept. |
| D11 | VAT: «З ПДВ» only when VAT is clearly printed; otherwise «Без ПДВ», also when unsure (user decision 2026-11-05). |

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | Register: tables, sheet read, sync («Оновити зараз» + daily step `purchase-invoices`), list with search and «Потребує перевірки» filter | implemented 2026-11-05 |
| 2 | Manual supplier link and note with optimistic version and history; filter «Без постачальника» | implemented 2026-11-05 |
| 3 | Invoice data: recognition of the original by OpenAI (Ai module), kopeck check (a mismatch → strong model, then «Потребує перевірки»), «Розпізнати всі нові» in batches, «Розпізнати знову», manual JSON, hash, history of replaced facts; move the one old checked document | implemented 2026-11-05 |
| 4 | Read-only «Рахунки на закупку» in the supplier card; «КП» and «Завдання в цех» in the client card | planned |
| 5 | Copies of the original documents on the Brevi CDN (`invoices/`): downloaded on sync, the card opens the copy; the Dropbox URL stays the identity (user request 2026-11-05) | planned |

## Stage 1 rules

- Header A1:N1 must be exactly `UID, ChatName, ChatId, MessageId, DateTime, FromUser, FromUserId, Text, MessageType, FileLinks, FileCount, Status, StatusDate, FromUserStatus`; the sheet id and title must match; the grid has 1–20 000 rows.
- Fully empty rows are skipped; a non-empty row without UID, ChatId or MessageId, a repeated UID, different payment marks within one message, or a UID that moved to another Telegram message stops the sync (422) and is recorded as the last error.
- Problems kept on the row (the row stays in the register): FileCount empty, not a whole number or different from the number of distinct links; a link that is not HTTPS Dropbox or has a user/password; FileLinks text without links; no original link.
- `MarkedPaid` only when trimmed, upper-cased Status equals «СПЛАЧЕНО». It is a note, not a payment.
- One card per message; the representative row is the active row with the smallest row number. Order: recognised message date (DD.MM.YYYY, DD/MM/YYYY, ISO) newest first, unknown dates last, then UID.

## Stage 2 rules

- `PUT /api/purchasing/invoices/{messageId}/review` takes `supplierId` (null unlinks), `note` (trimmed, empty removes, at most 1000) and the `version` the user saw. A stale version → 409 «Цей рахунок уже змінили…»; the newer change stays.
- A new link needs an active Reference supplier; a supplier that became inactive may stay linked. Unknown supplier or card → 404.
- Each real change adds 1 to the version, stores who (e-mail) and when, and appends a `ReviewChanges` entry with the values before and after. Saving the same values changes nothing.
- Reviews and the sheet sync share one advisory lock, so they never interleave; a review during a sync → 409 «Оновлення реєстру вже виконується…».
- The accountant cannot read the Reference supplier list, so the register offers `GET /api/purchasing/invoices/suppliers` (active suppliers by name).

## Stage 3 rules

- Facts JSON (`PurchaseDocumentFile` in the contract): `documentType`, `documentNumber`, `documentDate` (YYYY-MM-DD), `supplierName`, `vat` (`with_vat` / `without_vat`), `pricesIncludeVat`, `items` (1–200: `name`, `quantity` ≤ 4 places, `unit` or null, `price` ≤ 4 places, `sum` ≤ 2 places), `vatAmount` (required with VAT, otherwise null or 0), `totalPayable`. Decimals are strings with a dot; numbers in JSON are read as their text. Positions are numbered by the server.
- Check in kopecks: Σ sums = total, or Σ sums + VAT = total when the prices are without VAT. A line whose quantity × price differs from its sum by more than 1 грн and 1 % is a misreading; smaller gaps are rounded prices and pass. A sum up to 30 % below quantity × price is a line discount and passes when the totals add up (user 2026-11-05, file_798 with 5 %); a larger or higher gap stays a problem. An invoice without a «Ціна» column gives an empty price, and the price becomes sum ÷ quantity (user 2026-11-05).
- «Розпізнати» downloads the Dropbox original (`dl=1`, safe links only, ≤ 20 MB, PDF or picture; Excel and Word are «Skipped» with a reason), asks the fast model, retries once with the strong model, and records the attempt. Valid facts are saved with the model and cost; a recognised file can be recognised again, and the previous facts go to `CheckedDocumentHistory`.
- «Розпізнати всі нові» takes 5 files per call that were never tried (newest messages first); the admin repeats it while files remain. It stops when the AI is off, has no key or reached its monthly limit (503 for a single file; nothing is recorded).
- Problems of the last attempt of a file without facts are card issues («<file>: <problem>») and put it in «Потребує перевірки».
- Manual JSON: `sourceUID` must be an active row of the file's message and `originalFileLink` its exact link. Canonical JSON (fixed order) → SHA-256: the same facts change nothing; different facts need `replace = true`. `GET …/document-template` gives the saved facts, the last AI answer or empty fields as the start.
- `supplierName` from the document never links a Brevi supplier by itself.
- Recognition, saving and the sheet sync share the register lock; during a sync → 409.

## Contract and tests

[OpenAPI](../../../contracts/purchasing/purchase-invoices.openapi.yaml): `getPurchaseInvoices`, `getPurchaseInvoicesSyncState`, `syncPurchaseInvoices`, `getPurchaseInvoiceSuppliers`, `reviewPurchaseInvoice`, `recognizePurchaseFile`, `recognizeNewPurchaseFiles`, `savePurchaseDocument`, `getPurchaseDocumentTemplate`. Tests: `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Purchasing"` (source rules, sync on SQLite, list query, review, facts rules, recognition, manual JSON, contract) and `tests/ArchitectureTests/PurchasingDependencyTests.cs`.

## Deployment

The migrations `InitialPurchasing`, `AddPurchaseReview` and `AddPurchaseDocuments` run with the normal deploy. Stage 3 needs the OpenAI key in «Налаштування ШІ» ([AI-001](../../ai/001-ai-settings/README.md)). Before the first sync: enable Google Sheets API in project `brevistoreapi` and share the sheet with the runtime service account as a viewer ([runbook](../../../operations/jobs/sync-job.md#purchase-invoices)).
