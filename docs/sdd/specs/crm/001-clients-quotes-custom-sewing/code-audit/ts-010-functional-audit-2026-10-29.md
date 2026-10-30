# TS-009/TS-010 functional and file-role audit — 2026-10-29

**Scope:** the P5 backend code added for issuer/template settings, quote ready and private PDF artifacts. This is a follow-up review of the local implementation, not a release acceptance or a frontend audit.

## Evidence reviewed

- Domain `Documents/{Issuer,Templates}`, `Quotes/Issues` and the ready transition in `Quote.cs`;
- Application `Documents/{Projection,RenderQuotePdf,GetQuoteArtifact,QuoteArtifacts,Source}` and `Quotes/PrepareQuote`;
- Infrastructure document settings, numbering, artifact state/storage, authorized render source, EF mappings/migrations and DI;
- CRM HTTP controllers, Host.Api PDF worker, canonical ready/PDF OpenAPI, and focused tests.

The largest hand-written P5 production files are `QuotePdfArtifactStore.cs` (124 lines), `QuotePdfArtifact.cs` (114), `IssuerSettingsStore.cs` (106), `QuotePublicDocumentProjectionBuilder.cs` (82), `PrepareQuoteCommandHandler.cs` (74) and `QuotePdfWorker.cs` (74). Generated EF migration designers are excluded from file-size decisions.

## Findings and resolution

| ID | Severity/status | Evidence and impact | Owner, target change, dependency | Verification |
| --- | --- | --- | --- | --- |
| AF-CRM-P5-003 | P3, closed | `Crm.Api/Crm.Api/Quotes/QuoteLifecycleController.cs` held P3 history/reprice/copy/transfer/archive plus the P5 ready HTTP action. This made P5 document readiness depend on a controller with a separate lifecycle remit; subsequent changes to one flow would expand the other file. | CRM API; moved only `POST /api/crm/quotes/{quoteId}/ready` into `Quotes/QuoteReadyController.cs`, keeping route name, role policy, request type, command and response mapping. No contract or migration change. | `QuoteReadyApiTests` passed 1/1: anonymous 401, User 403, Manager command dispatch and 409 mapping; previous P5 ready PostgreSQL tests passed 4/4. |
| AF-CRM-P5-004 | P3, closed | `docs/sdd/features/crm/clients-and-quotes.md` still described ready/numbering as future work in its P3 section; its `Tests` section appeared before the newly added P5 implementation section. Readers could miss the actual P5 route and focused tests. | CRM feature map; corrected the P3 status, named both P5 controllers and request, and moved `Tests` to the end with the ready HTTP regression. Depends on AF-CRM-P5-003. | File paths checked against the repository and `git diff --check` passed. |

## Boundary decision

No further production split is justified in this slice. `IssuerSettingsStore` keeps create/activate mutations in one transaction owner with a shared advisory lock and idempotency journal; its active read already delegates to `DocumentSettingsReadStore`. `QuotePdfArtifactStore` keeps queue, claim, retry, completion and saved-byte retrieval under one artifact persistence owner, while its state rules live in `QuotePdfArtifact` and public reads in separate Application handlers. `QuoteDocumentSource` owns pinned-source loading and quote-scope checks; the worker owns polling/render orchestration; the renderer and logo resolver are separate adapters. Quote aggregate invariants remain in the Domain root. These boundaries match the current feature map and do not require another folder or interface merely because several related files exist.

The scoped refactor preserved the published route and did not change the PostgreSQL schema, artifact format, numbering policy or historical PDF behavior. TS-010's combined PostgreSQL/Chromium proof is recorded in [execution evidence](../evidence/ts-010-2026-10-29.md); frontend handoff and deployment remain outside this audit.
