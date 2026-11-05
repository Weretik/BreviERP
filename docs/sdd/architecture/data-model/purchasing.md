# Purchasing schema

Code model reviewed 2026-11-05. `Purchasing.Infrastructure.Persistence.PurchasingDbContext` owns schema `purchasing`; migration history is `purchasing.__EFMigrationsHistory`. Migration `InitialPurchasing` creates all tables ([PUR-001](../../specs/purchasing/001-purchase-invoices/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| Messages | PK Id: SHA-256 hex of `telegram:<TelegramKey>`. Unique TelegramKey (`ChatId:MessageId`). Source fields MessageAt (text), MessageDate (for sorting), Text, StatusOriginal, MarkAt, MarkBy, PaymentState; manual SupplierId (Reference supplier id, no FK), ReviewNote, ReviewVersion; UpdatedAt changes only with source fields |
| SourceRows | PK Id: SHA-256 of `<spreadsheetId>:<sheetId>:<UID>`. Unique SourceUid; MessageId FK → Messages (restrict). RowNumber, RawJson (exact JS JSON text), Issues (text[]), Active, SeenRun, UpdatedAt |
| SourceRevisions | PK Id: SHA-256 of `<sourceId>:<rawJson>`; SourceId FK → SourceRows (restrict). RawJson, At. Append-only |
| MessageFiles | PK Id: SHA-256 of `<messageId>:<url>`; MessageId FK → Messages (restrict); unique (MessageId, Url). Active, SeenRun, State, Issue. Only the link is stored, not the file |
| SyncStates | PK Id (`google-finance`). LastAttemptAt, LastSuccessAt, Error, RecordCount (active rows), FileCount |

Nothing is deleted: rows and links missing from the sheet get `Active = false`.
