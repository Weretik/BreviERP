# CRM clients, contacts and draft quotes

**Implemented local slices:** clients, P2/P3 quotes, P4 custom drafts/files/templates/suggestions and P5 TS-009 issuer settings plus TS-010 ready/private PDF artifact backend, verified locally 2026-10-29. Consumer FE-006/007 is automatically verified against backend `fdb8095d4dfd52a9680b5a2768c706aea1aa8c4b`; the newer P5 API has not been handed to frontend. [P4 checkpoint](../../specs/crm/001-clients-quotes-custom-sewing/evidence/p4-complete-2026-10-28.md), [P5 TS-010 evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-010-2026-10-29.md). Import, send and production rollout remain future phases.

Authenticated Admin/Manager can create, read, search/page, revise and deactivate customers and contacts. Admin-only merge retains victim roots/audit while moving contacts/external identities to the survivor. Exact external identities are unique; similar names/shared emails never auto-merge. Manager reads/writes owned clients; draft quotes are owner-scoped. Every mutation has requestId replay and a root revision; successful intent/result audit is stored in the same transaction. No destructive customer/contact delete API.

## Code structure

```text
src/Modules/Crm/
  Crm.Domain/Crm.Domain/Customers/
    Customer.cs                       root invariants/revision/merge
    Identifiers/                      typed long keys and int actor reference
    Contacts/                         contact and channel validation
    ExternalIdentities/               exact source identity
  Crm.Application/Crm.Application/Customers/
    Access/                           authenticated actor reference
    Contracts/Inputs/                 shared customer/source intent types
    Contracts/Responses/              pure detail/contact/source response records
    Mapping/                          domain-to-detail mapping
    GetCustomers/DTOs/                page summary projection
    SaveCustomerContact/Inputs/       contact/channel intent types
    Persistence/                      transaction session and read ports
    GetCustomer/Specifications/       scoped SQL detail projection
    GetCustomers/Specifications/      SQL page/count/search projections
    CreateCustomer/                   command, handler, validator
    ReviseCustomer/                   command, handler, validator
    SaveCustomerContact/              create/revise contact use case
    MergeCustomers/                   Admin merge orchestration
  Crm.Infrastructure/Crm.Infrastructure/
    Persistence/                      EF context, entity configurations/migrations
    Persistence/Customers/Reading/     scoped read repository
    Persistence/Customers/Writing/     transaction coordinator and root locks
    Persistence/Customers/Idempotency/ payload encoding, replay/access and journal
    DependencyInjection/              existing host/seed registration
  Crm.Api/Crm.Api/Customers/
    CustomersController.cs            client reads/writes/merge
    CustomerContactsController.cs     contact writes
    Requests/                         one request type per file
  Crm.Api/Crm.Api/Access/
    CrmActorClaims.cs                 actor binding shared by CRM controllers
```

```mermaid
flowchart LR
  API[Authenticated Crm controllers] --> CQRS[Application handlers and validators]
  CQRS --> Root[Customer/contact invariants]
  CQRS --> Read[Scoped SQL specifications]
  CQRS --> Write[Mutation transaction port]
  Read --> PG[(crm schema)]
  Write --> Lock[Root/request locks and revision check]
  Lock --> PG
  Lock --> Audit[CustomerMutationJournal: replay and atomic audit]
  Audit --> PG
```

[Actual API contract](../../contracts/crm/clients-quotes.openapi.yaml), [database ownership](../../architecture/data-model/crm.md), [local migration job](../../operations/data/crm-clients.md). Host.Api registers the Crm Mediator assembly and infrastructure; Host.Seed registers Crm migration in normal deployment flow and an explicit CRM-only local mode. Existing Catalog/Reference/Identity remain separate owners.

[Structural audit and verification](../../specs/crm/001-clients-quotes-custom-sewing/code-audit/audit.md). The transaction coordinator retains commit/rollback ownership; the journal uses the same context and transaction. Customer aggregate invariants remain together; mapped entity/context namespaces and the HTTP/JSON contract are unchanged.

## Draft quote structure

```text
Crm.Domain/Crm.Domain/Quotes/
  Quote.cs, Versions/, Items/, Variants/, Pricing/, Identifiers/
Crm.Application/Crm.Application/Quotes/
  CreateQuote/, SaveQuoteVersion/, GetQuote/, GetQuotes/
  PreviewQuotePrices/Specifications/  preview-specific access query
  Pricing/QuotePriceBasisResolver.cs  token, input, expiry and source checks
  Mapping/QuoteVersionBuilder.cs      ordered graph construction
  Contracts/Inputs/, Contracts/Responses/, Persistence/
Crm.Infrastructure/Crm.Infrastructure/Persistence/
  Quotes/Reading/, Writing/, Pricing/
  Quotes/Idempotency/                 hash, replay/access and atomic journal
  Configurations/Quotes/             seven quote-specific EF mappings
Crm.Api/Crm.Api/Quotes/               HTTP binding and separate request records
Host.Api/Crm/QuoteCatalogPriceReader.cs
tests/IntegrationTests/Crm/Quotes/    API, adapter, persistence, access, integrity
```

```mermaid
flowchart LR
  API[Quote controller] --> Preview[Price preview handler]
  API --> Write[Create/save handler]
  API --> Read[Owner-scoped read handlers]
  Preview --> Catalog[Host adapter to Catalog pricing]
  Preview --> Bases[Persisted expiring basis]
  Write --> Builder[Version builder]
  Builder --> Resolver[Price basis resolver]
  Resolver --> Bases
  Resolver --> Catalog
  Write --> Tx[QuoteMutationStore transaction]
  Tx --> Journal[QuoteMutationJournal replay/access]
  Tx --> PG[(crm quote graph and composite version FKs)]
  Journal --> PG
  Read --> PG
```

Drafts preserve saved monetary snapshots across reads and text edits. Fresh pricing tokens are actor/quote/input-bound; source drift conflicts. See the [quote contract](../../contracts/crm/quote-drafts.openapi.yaml), [P2 audit](../../specs/crm/001-clients-quotes-custom-sewing/code-audit/p2-implementation-audit-2026-10-26.md) and [verification evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/p2-quote-backend-2026-10-26.md).

## P3 lifecycle implementation

```text
Crm.Domain/Crm.Domain/Quotes/
  Quote.cs, Events/QuoteLifecycleEvent.cs    revision, ownership/archive, issued pointer and audit
Crm.Application/Crm.Application/Quotes/
  GetQuoteVersions/, GetQuoteVersion/, GetQuoteEvents/    scoped paged typed history
  RepriceQuote/                              confirmation of all current preview tokens
  RepriceQuote/Mapping/QuoteRepriceSnapshot.cs saved graph conversion, separate from orchestration
  GetQuoteVersions/Specifications/, GetQuoteEvents/Specifications/ scenario-owned access checks
  CopyQuote/, TransferQuote/, ArchiveQuote/   independent command/handler per action
  Mapping/QuoteSnapshotCloner.cs              deep copy of saved graph without pricing
  Ownership/IQuoteAssigneeReader.cs           eligibility port
Crm.Infrastructure/Crm.Infrastructure/Persistence/
  Quotes/Reading/QuoteReadRepository.History.cs          SQL history projections
  Configurations/Quotes/QuoteLifecycleEventConfiguration.cs
  Migrations/*AddQuoteLifecycle*, *PreserveIssuedQuoteVersion*
Crm.Api/Crm.Api/Quotes/QuoteLifecycleController.cs
Crm.Api/Crm.Api/Quotes/QuoteAssigneesController.cs separate eligible-user lookup HTTP boundary
Host.Api/Crm/QuoteAssigneeReader.cs            Identity eligibility adapter
```

```mermaid
flowchart LR
  API[Quote lifecycle controller] --> History[Scoped version and event queries]
  API --> Commands[Copy / transfer / archive / reprice handlers]
  Commands --> Tx[Existing mutation transaction and replay journal]
  Commands --> Identity[Assignee eligibility port via Host adapter]
  Commands --> Snapshot[Saved graph clone or fresh token resolver]
  Tx --> Root[Quote revision and append-only events]
  Root --> PG[(crm immutable versions and root pointers)]
  History --> PG
```

Reprice confirms an unchanged current graph using all fresh scoped variant tokens. Copy selects any authorized saved version, records its provenance and preserves prices without a Catalog call. Transfer atomically changes current access; former Managers cannot read history or replay prior mutations after losing scope. Archive/restore preserve business state and versions. Ready/sent edits create a draft and retain the issued-version pointer. The ready operation and human numbering are implemented in P5; sending remains a later phase. [TS-005 evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-005-2026-10-28.md). Frontend lifecycle integration is pending.

## Private custom sewing

Existing Admin/Manager authentication protects every custom operation. Structured draft input stores name/type/season, public/private text, approved application sale intent and component selections/norms. Explicit server calculation records approved source versions and a scoped expiring basis token; confirmation freezes a new immutable version. Quote selection uses the saved version ID and never posted costs. History/copy/text edits preserve the old configuration and prices. [Protocol](../../specs/crm/001-clients-quotes-custom-sewing/design/custom-sewing-protocol.md).

```text
Crm.Domain/Crm.Domain/CustomSewing/
  Composition/, Drafts/, Versions/, Identifiers/, Pricing/  bounded configuration and immutable source basis
Crm.Application/Crm.Application/CustomSewing/
  CreateDraft/, SaveDraft/, Calculate/, SaveVersion/        independent commands, handlers, validators
  GetDraft/, GetVersion/, GetVersions/                     scenario-local SQL projections and paging
  ResolveQuoteVersions/Specifications/                    quote-selected private version scope
  Mapping/CustomDetailProjection.cs                       pure shared response projection
  Access/, Contracts/, Persistence/, Pricing/              policies, views and ports
Crm.Infrastructure/Crm.Infrastructure/Persistence/
  CustomSewing/{Reading,Writing,Pricing,Idempotency}/        SQL, locking, basis storage and replay journal
  Configurations/CustomSewing/                             typed JSONB converters and constraints
  Migrations/*AddCustomSewingVersions*, *EnforceCustomSewingHistory*
Crm.Api/Crm.Api/CustomSewing/                               strict request binding and scoped controller
Host.Api/Crm/CustomSewing/
  Pricing/CustomSewingPricing.cs                           Catalog evaluator coordinator
  Mapping/{CustomPricingInputMapper,CustomPriceBasisMapper}.cs  typed input/result translation
Crm.Domain/Crm.Domain/CustomSewing/Pricing/CustomCalculationReadiness.cs  eligibility and tier bounds
Crm.Infrastructure/Crm.Infrastructure/Persistence/CustomSewing/Pricing/CustomCalculationStore.cs  scoped token persistence
Crm.Application/Crm.Application/Quotes/                    custom preview and server-side snapshot hydration
```

```mermaid
flowchart LR
  HTTP[CustomSewingController] --> CQRS[Draft / calculate / version handlers]
  CQRS --> Tx[Custom mutation transaction and replay journal]
  CQRS --> Pricing[Host pricing bridge]
  Pricing --> Catalog[Catalog custom evaluator and shared arithmetic]
  Pricing --> Mapping[Typed input and result mappers]
  Mapping --> Policy[Crm Domain calculation readiness]
  Tx --> Tokens[Scoped custom calculation store]
  Tx --> Root[Mutable custom draft]
  Tx --> Version[Immutable custom version and protected history]
  HTTP --> Reads[Scoped projected history]
  Version --> Select[Quote custom intent with version ID]
  Select --> Batch[Scoped batch hydration]
  Batch --> Snapshot[Quote numeric FK and saved configuration / prices]
```

Additional focused tests: `tests/UnitTests/Crm/CustomSewing/CustomSewingDraftTests.cs`, `CustomSewingContractTests.cs`, `tests/UnitTests/Crm/Quotes/QuoteTierProjectionTests.cs`; `tests/IntegrationTests/Crm/CustomSewing/{CustomSewingPersistenceTests,CustomSewingApiTests,CustomSewingPricingBridgeTests}.cs`. Run `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~CustomSewing` with the isolated local CRM test connection. Existing quote draft/lifecycle/access/migration regressions are also covered. [Dated verification](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-006-2026-10-28.md).

[TS-006 functional audit and decomposition](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-006-audit-refactor-2026-10-28.md): confirmation rechecks expiry after source validation, Host mapping/orchestration is split, eligibility belongs to Domain and calculation-token persistence has its own store. Current audit verification: 36 unit, 17 integration/API and 2 architecture tests passed.

## Configuration templates, suggestions and private adapter readiness

TS-008 implements scoped versioned customConfiguration templates with introduction/selections/norms, archive/restore and exact request replay. Apply creates a new private draft with retained TemplateOrigin and no implicit pricing. Source-style suggestions use approved unique models, six exact garment groups, sample coverage and varying norms; four bounded Catalog SQL reads plus one reference resolution avoid per-article queries. No supplier costs or private notes enter the reusable/public projection. [Contract](../../contracts/crm/custom-templates.openapi.yaml), [evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-008-2026-10-28.md).

Code structure: Domain CustomSewing/Templates and Drafts/CustomTemplateOrigin; Application CustomSewing/{CreateTemplate,ReviseTemplate,ArchiveTemplate,GetTemplate,GetTemplates,ApplyTemplate,GetSuggestions}, with suggestion DTOs in GetSuggestions/Contracts; Infrastructure Persistence/Templates/{Reading,Writing,Idempotency} and Configurations/Templates; API Templates/{controllers,Requests}; Catalog Contracts/Suggestions and Infrastructure/Suggestions; Host.Api/Crm/CustomSewing/Suggestions with pure translation in Mapping/SewingSuggestionInputMapper.cs. Shared DI wires these capabilities. AddCustomSewingTemplates adds three tables and nullable draft origin; immutable template history rejects updates/deletes.

EN-003 separates Application Abstractions/{Files,Documents} and Files/{Access,Validation} from Infrastructure Files/Storage and Documents/Rendering. Real private bytes, allowlist/limits/hash/ACL and a six-page Ukrainian PDF are proven; registration is opt-in until dependent workflows supply authoritative access and issuance. [Configuration/runtime/retention design](../../specs/crm/001-clients-quotes-custom-sewing/design/private-adapters.md). Existing public Catalog storage is unchanged.

Focused tests: CustomTemplateRulesTests, SewingSuggestionRulesTests, PrivateFilePolicyTests; CustomTemplatePersistenceTests, CustomTemplateApiTests, TemplateQueryTranslationTests, SewingSuggestionBridgeTests, SewingSuggestionBatchBoundaryTests, PrivateFileStoreTests and ChromiumDocumentRendererTests. [Latest wave evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/p4-wave1-2026-10-28.md): 58 unit, 40 integration/API/adapter and 2 architecture checks passed, with explicit runtime/database configuration. Remaining P4 implementation: TS-007, FE-006, FE-007.

[Follow-up functional/structure audit](../../specs/crm/001-clients-quotes-custom-sewing/evidence/p4-wave1-audit-refactor-2026-10-28.md): 40 focused checks passed after mapping/journal separation and DTO grouping. Renderer admission now also bounds font/HTML preparation; cancelled queued calls do not prepare content. No schema/API change.

## Private file lifecycle — TS-007

POST files stages real bytes with known reserved IDs, bounded MIME/size/hash checks, actor/request replay and root quota. Explicit PUT custom draft files uses the root revision/journal and independent ShowInQuote/Production flags. SaveVersion pins immutable same-draft file metadata/flags in the existing transaction; later draft changes cannot alter a quoted version. GET content requires finalized current or exact authorized quoted-version references and checks actual bytes/hash. Quote copy/transfer grants no access to unrelated source versions. Bounded temporary cleanup is an internal explicit service and excludes current/history references; no deletion worker is enabled.

Code structure: Domain CustomSewing/Attachments; Application Files/{Upload,Finalize,GetContent,GetFiles,Contracts,Storage,Reading,Verification,Cleanup}; Infrastructure Files/{Upload,Verification,Finalize,Reading,Access,Versions,Cleanup,Storage}; API Files/Requests; Persistence/Configurations/Files. Scenario specs remain in each use case. CustomSewingPriceCalculator requires exact scoped artwork proof before verified pricing; no-artwork flows retain the previous path. AddCustomPrivateFileLifecycle creates three tables with same-draft FKs and append-only version-file history. [Actual schema](../../architecture/data-model/crm.md), [file contract](../../contracts/crm/private-files.openapi.yaml).

Host.Api/Crm/Files/CrmFileServices wires lazy private storage from the actual environment public root and Crm:PrivateFiles:RootPath; no writable-volume default. Missing configuration yields safe 503 only for file operations. TemporaryLifetimeMinutes defaults1440 for unattached uploads, with accepted range1–10080; historical retention remains EN-006. No public media fallback, new package or pricing formula. [Task/configuration and 120-case evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-007-2026-10-28.md).

[Requested follow-up audit](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-007-audit-refactor-2026-10-28.md): Files/Cleanup/CustomTemporaryFileCleanup coordinates bounded byte deletion; TemporaryFileCleanupStore owns claim/reference/complete SQL transitions; TemporaryFileCleanupClaim holds typed identity/provenance. Same context/locks and no byte deletion inside transactions. Fresh9 PostgreSQL +2 architecture checks passed; no EF model/API change.

## Issuer and document settings — TS-009

Admin creates immutable paired versions of seller requisites and structured document text, then activates the pair with expected revision and idempotent requestId. Manager can read the active pair. No settings are seeded or active by default. The logo field is a logical allowlisted key; its bytes and ready/PDF validation belong to TS-010. A monthly UTC allocator, called inside the future ready transaction, serializes numbers `N/M/YYYY` with a database advisory lock and accounts for quote numbers already present in either legacy or new format. [Contract](../../contracts/crm/issuer-settings.openapi.yaml), [checkpoint](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-009-2026-10-29.md).

```text
Crm.Domain/Crm.Domain/Documents/{Issuer,Templates}/       immutable versions and typed IDs
Crm.Domain/Crm.Domain/Documents/                    active pair and revision
Crm.Application/Crm.Application/Documents/{GetIssuerSettings,CreateIssuerSettingsVersion,ActivateIssuerSettings}/
Crm.Application/Crm.Application/Documents/{Contracts,Persistence,Numbering}/
Crm.Infrastructure/Crm.Infrastructure/Persistence/Documents/
  Writing/IssuerSettingsStore.cs                     create/activate transactions
  Reading/DocumentSettingsReadStore.cs               active pair read
  Mapping/DocumentSettingsMapper.cs                  response projection
  Idempotency/DocumentSettingsMutationJournal.cs     request hash/replay and journal
  Numbering/DocumentNumberAllocator.cs               transactional monthly allocation
Crm.Infrastructure/Crm.Infrastructure/Persistence/Configurations/Documents/  EF mappings and constraints
Crm.Infrastructure/Crm.Infrastructure/Persistence/Migrations/*AddIssuerSettingsAndNumberSequence*
Crm.Api/Crm.Api/Documents/                         role-protected settings controller and requests
```

```mermaid
flowchart LR
  API[Issuer settings API] --> Cases[Get / create / activate handlers]
  Cases --> Store[Settings store and replay journal]
  Store --> Versions[Immutable issuer and template versions]
  Store --> Active[Active pair revision]
  Ready[Ready transaction] --> Allocator[Monthly number allocator]
  Allocator --> Quotes[Known quote numbers and sequence]
```

**Data:** [Crm schema](../../architecture/data-model/crm.md). TS-009 isolated PostgreSQL integration passed five focused tests; its migration is applied to local `brevidb` only.

## P5 ready and private PDF artifact — TS-010

The ready command validates an existing saved version and visible public price selection, checks active issuer/template settings and server-managed logo availability, and atomically pins a `QuoteIssue` with the human number. Rendering is a separate explicit request: it queues one versioned private artifact and returns status. Authorized metadata/content reads use the pinned issue and saved PDF bytes; download does not render, reprice or mark sent. The hosted worker and real Chromium PDF are tested as separate boundaries in [TS-010 evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-010-2026-10-29.md). [Structural audit](../../specs/crm/001-clients-quotes-custom-sewing/code-audit/ts-010-functional-audit-2026-10-29.md) records the controller split and file-role review.

```mermaid
flowchart LR
  ReadyApi[Quote ready API] --> Prepare[PrepareQuote handler]
  Prepare --> Check[Ready validator and brand asset]
  Prepare --> Tx[Quote mutation transaction and number allocator]
  Tx --> Issue[Immutable QuoteIssue pin]
  PdfApi[Quote PDF API] --> Source[Authorized pinned document source]
  Source --> Artifact[Private artifact queue and status]
  Artifact --> Worker[Configured renderer worker]
  Worker --> Projection[Allowlisted quote and issuer projection]
  Projection --> Pdf[Saved PDF bytes and SHA-256]
  PdfApi --> Pdf
```

```text
src/Modules/Crm/
  Crm.Domain/Crm.Domain/Quotes/{Quote.cs,Issues/QuoteIssue.cs}            ready transition and immutable pin
  Crm.Application/Crm.Application/Quotes/PrepareQuote/                 command, validation and transaction orchestration
  Crm.Application/Crm.Application/Documents/{Projection,Source}/       public allowlist and pinned source port
  Crm.Application/Crm.Application/Documents/{RenderQuotePdf,GetQuoteArtifact,Artifacts}/
                                                                      queue/read use cases and private artifact contract
  Crm.Infrastructure/Crm.Infrastructure/Persistence/Documents/{Reading,QuoteArtifacts}/
                                                                      scoped issue source and artifact lifecycle/bytes
  Crm.Infrastructure/Crm.Infrastructure/Persistence/Configurations/{Quotes,Documents}/
                                                                      EF relationships and constraints
  Crm.Infrastructure/Crm.Infrastructure/Persistence/Migrations/*AddQuoteIssues*,*AddQuoteArtifactIssuePin*
  Crm.Api/Crm.Api/Quotes/QuoteReadyController.cs                       ready HTTP boundary
  Crm.Api/Crm.Api/Quotes/Requests/PrepareQuoteRequest.cs               ready request
  Crm.Api/Crm.Api/Documents/QuotePdfController.cs                      PDF artifact HTTP boundary
```

Canonical [quote and ready](../../contracts/crm/quote-drafts.openapi.yaml) and [PDF artifact](../../contracts/crm/quote-pdf.openapi.yaml) operations are local, uncommitted P5 changes. The frontend still pins the P4 backend SHA. Local `brevidb` has both additive TS-010 migrations applied after backup; production and FE-008 integration remain separate.

PDF runtime configuration requires `Crm:Documents:{ChromiumPath,FontPath,PrivateWorkingRootPath}`. A selected `brevi-wordmark` additionally requires a provisioned local PNG/JPEG at `Crm:Documents:WordmarkPath`; ready blocks if it is unavailable. No arbitrary image URL is fetched.

## Tests

TS-009: `tests/UnitTests/Crm/Documents/IssuerSettingsRulesTests.cs`, `tests/IntegrationTests/Crm/Documents/IssuerSettingsRequestContractTests.cs` and `IssuerSettingsPersistenceTests.cs`. Focused commands: `dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~IssuerSettingsRulesTests` and `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~IssuerSettings`. The request contract test runs without PostgreSQL; persistence tests require the isolated local CRM test connection. [Audit](../../specs/crm/001-clients-quotes-custom-sewing/code-audit/ts-009-functional-audit-2026-10-29.md). Earlier client, quote, custom and file test paths are recorded in their sections above.

TS-010: `tests/UnitTests/Crm/{Quotes,Documents}/`, `tests/IntegrationTests/Crm/Quotes/{QuoteReadyApiTests,QuoteReadyPersistenceTests}.cs` and `tests/IntegrationTests/Crm/Documents/Quote{PdfApi,PdfArtifactPersistence,PdfArtifactState,PdfProjectionRenderer,PdfWorker,DocumentSource}Tests.cs`. Focused HTTP regression after the controller split: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~QuoteReadyApiTests`. PostgreSQL and Chromium tests require their local test connection and runtime paths; exact combined verification is in [TS-010 evidence](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-010-2026-10-29.md).

## First-contact selections — TS-016 backend checkpoint 2026-10-30

Domain `Quotes/Introductions/` owns layouts, versioned selections/featured mappings and public snapshots. Application `Quotes/Introductions/` owns template operations, current-price quote creation, separate IntroductionPriceBasisIssuer for per-slot basis issuance, and stable application intent/replay port. Infrastructure `Persistence/Introductions/` separates reads, writes and mutation journal; public snapshots map to QuoteVersions jsonb. API `Templates/Introduction*Controller.cs` exposes owned template operations and Admin featured activation; Host `Crm/IntroductionSourceMappingReader.cs` resolves exact imported source identities. Documents use the shared projection/Chromium pipeline. [Canonical contract](../../contracts/crm/introduction-templates.openapi.yaml), [verification and rollout](../../specs/crm/001-clients-quotes-custom-sewing/evidence/ts-016-backend-gates-2026-10-30.md). Consumer integration and rendered acceptance remain open.
