# Purchasing schema

Code model reviewed 2026-11-05. `Purchasing.Infrastructure.Persistence.PurchasingDbContext` owns schema `purchasing`; migration history is `purchasing.__EFMigrationsHistory`. Migration `InitialPurchasing` creates the register tables; `AddPurchaseReview` adds ReviewedBy/ReviewedAt and ReviewChanges; `AddPurchaseDocuments` adds CheckedDocuments, CheckedDocumentHistory and RecognitionAttempts; `AddPurchaseDocumentFilters` adds CheckedDocuments.DocumentDate and Vat, filled from PayloadJson; `AddPurchasePayments` adds Payments ([PUR-001](../../specs/purchasing/001-purchase-invoices/README.md)).

| Table | Identity / relationships / purpose |
| --- | --- |
| Messages | PK Id: SHA-256 hex of `telegram:<TelegramKey>`. Unique TelegramKey (`ChatId:MessageId`). Source fields MessageAt (text), MessageDate (for sorting), Text, StatusOriginal, MarkAt, MarkBy, PaymentState; manual SupplierId (Reference supplier id, no FK), ReviewNote, ReviewVersion, ReviewedBy (e-mail), ReviewedAt; UpdatedAt changes only with source fields |
| SourceRows | PK Id: SHA-256 of `<spreadsheetId>:<sheetId>:<UID>`. Unique SourceUid; MessageId FK → Messages (restrict). RowNumber, RawJson (exact JS JSON text), Issues (text[]), Active, SeenRun, UpdatedAt |
| SourceRevisions | PK Id: SHA-256 of `<sourceId>:<rawJson>`; SourceId FK → SourceRows (restrict). RawJson, At. Append-only |
| MessageFiles | PK Id: SHA-256 of `<messageId>:<url>`; MessageId FK → Messages (restrict); unique (MessageId, Url). Active, SeenRun, State, Issue. Only the link is stored, not the file |
| ReviewChanges | PK Id (identity); MessageId FK → Messages (restrict); index (MessageId, Id). Reviewer (e-mail), At, Before*/After* SupplierId, Note, Version. Append-only history of the manual supplier link and note |
| CheckedDocuments | PK Id = file id (one per file); FK → MessageFiles (restrict); MessageId FK → Messages (restrict). SourceUid, OriginalUrl, PayloadJson (canonical facts JSON), PayloadHash (SHA-256 of it), DocumentDate (indexed) and Vat (WithVat/WithoutVat) copied from it for the register filters, Origin (Ai/Manual), Model, CostUsd, CheckedBy, CheckedAt |
| CheckedDocumentHistory | PK Id (identity); FileId FK → MessageFiles (restrict); index (FileId, Id). The replaced PayloadJson/PayloadHash/Origin/CheckedBy/CheckedAt, ReplacedBy, ReplacedAt. Append-only |
| RecognitionAttempts | PK Id (identity); FileId FK → MessageFiles, MessageId FK → Messages (restrict); index (FileId, Id). Outcome (Recognized/NeedsReview/Failed/Skipped), Problems (text[]), Model, CostUsd, OutputJson (last AI answer that failed the checks), By, At. Append-only |
| Payments | PK Id (identity); MessageId FK → Messages (restrict); index (MessageId, Id), index TransactionId; unique (MessageId, TransactionId) while UnlinkedAt is null. TransactionId (Accounting operation id, no FK), LinkedBy, LinkedAt, UnlinkedBy, UnlinkedAt. A removed link stays as history |
| SyncStates | PK Id (`google-finance`). LastAttemptAt, LastSuccessAt, Error, RecordCount (active rows), FileCount |

Nothing is deleted: rows and links missing from the sheet get `Active = false`.
