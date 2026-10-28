# Proposed public and internal contracts

**Local contract status:** seven customer/contact/merge operations are implemented in canonical [client OpenAPI](../../../../contracts/crm/clients-quotes.openapi.yaml); five P2 draft-quote operations are implemented in [quote OpenAPI](../../../../contracts/crm/quote-drafts.openapi.yaml) and registered in the [aggregate](../../../../contracts/openapi.yaml). Later quote lifecycle/custom/document operations below remain proposed. Their operation IDs are reserved by this SDD, not claims of existing endpoints. Implemented schemas use nested input DTOs, server-owned actor, requestId and expectedRevision where applicable. Invalid input maps to 400 under existing Result mapping; no new 422 middleware was introduced. The frontend snapshot pins these contracts at `1c1f51dc7837f3d5e3a0da7326cb17b48d23e4d9`; see [handoff](../evidence/p2-handoff-2026-10-27.md).

## Existing dependencies

- [Product catalog](../../../../contracts/catalog/product-catalog.openapi.yaml): `getAdminProducts`, `getAdminProductById` for metadata, not deprecated price placeholders.
- [Sewing composition](../../../../contracts/catalog/sewing-composition.openapi.yaml) supplies Article identity/composition; select the actual operation ID from that contract when implementing the picker.
- [Sewing pricing](../../../../contracts/catalog/sewing-pricing.openapi.yaml): `getPublishedSewingPrice`, `getSewingCalculation`, `getPublishedSewingProducts`; existing `saveSewingCalculation` / `publishSewingCalculation` remain Catalog publication operations, **not** private-quote save operations.
- [PPE pricing](../../../../contracts/catalog/ppe-pricing.openapi.yaml): canonical current-price dependency. Confirm resolved operation ID/schema in EN-002; no duplicated quote formula or use of legacy Product.Price fields.

New Crm private calculation needs a server-side Application pricing port: input kind + article/custom selection refs + typed quantities + requested basis; output immutable resolved lines/input versions/settings/formula version, costs/tiers/blockers and basis ID. Extend/reuse the pure Catalog engine through a Host adapter. No direct internal HTTP loopback or cross-context EF query. Draft/quote reads must not trigger that calculation port.

The P2 [basis protocol design](../design/quote-preview-protocol.md) owns token binding, source identity and no-reprice rules. D-11 confirms UAH and no additional VAT calculation for draft prices. This is a design checkpoint, not a live contract. Freeze exact wire fields and errors in canonical OpenAPI when TS-003 is implemented.

## Operation registry

Paths below are proposed under `/api/crm`. All resource IDs in URLs are PublicId UUIDs. Literal routes (`suggestions`, `price-previews`) must not conflict with parameter routes; constrain UUID route parameters. One implementation task may contain several transport operations for a single capability; each command/query/handler still has one primary responsibility.

| Method | Proposed path                                              | Proposed operationId             | Boundary shape                                           | Owner task | Scenarios              |
| ------ | ---------------------------------------------------------- | -------------------------------- | -------------------------------------------------------- | ---------- | ---------------------- |
| GET    | `/api/crm/customers`                                       | `getCrmCustomers`                | CustomerListQuery → paged CustomerSummary                | TS-002     | SC-C01, SC-C04         |
| POST   | `/api/crm/customers`                                       | `createCrmCustomer`              | CreateCustomerRequest → CustomerDetail                   | TS-002     | SC-C01, SC-C02         |
| GET    | `/api/crm/customers/{customerId}`                          | `getCrmCustomer`                 | PublicId → CustomerDetail                                | TS-002     | SC-C01, SC-C04         |
| PUT    | `/api/crm/customers/{customerId}`                          | `reviseCrmCustomer`              | ReviseCustomerRequest → CustomerDetail                   | TS-002     | SC-C03, SC-C04         |
| POST   | `/api/crm/customers/{customerId}/contacts`                 | `createCrmCustomerContact`       | CreateContactRequest → ContactDetail                     | TS-002     | SC-C01, SC-C03         |
| PUT    | `/api/crm/customers/{customerId}/contacts/{contactId}`     | `reviseCrmCustomerContact`       | ReviseContactRequest → ContactDetail                     | TS-002     | SC-C03                 |
| POST   | `/api/crm/customers/{customerId}/merge`                    | `mergeCrmCustomers`              | MergeCustomersRequest → CustomerDetail                   | TS-002     | SC-C05                 |
| GET    | `/api/crm/quotes`                                          | `getCrmQuotes`                   | QuoteListQuery → paged QuoteSummary                      | TS-004     | SC-Q05                 |
| POST   | `/api/crm/quotes`                                          | `createCrmQuote`                 | CreateQuoteRequest → QuoteDetail                         | TS-004     | SC-Q01                 |
| GET    | `/api/crm/quotes/{quoteId}`                                | `getCrmQuote`                    | PublicId → QuoteDetail + capabilities                    | TS-004     | SC-Q01, SC-Q05         |
| POST   | `/api/crm/quotes/price-previews`                           | `previewCrmQuotePrices`          | QuotePriceInput → PricePreview/basisToken/blockers       | TS-003     | SC-Q02, SC-Q03, SC-Q07 |
| POST   | `/api/crm/quotes/{quoteId}/versions`                       | `saveCrmQuoteVersion`            | SaveQuoteVersionRequest → QuoteDetail                    | TS-004     | SC-Q01, SC-Q03, SC-Q04 |
| GET    | `/api/crm/quotes/{quoteId}/versions`                       | `getCrmQuoteVersions`            | VersionListQuery → paged VersionSummary                  | TS-005     | SC-Q02, SC-Q05         |
| GET    | `/api/crm/quotes/{quoteId}/versions/{versionId}`           | `getCrmQuoteVersion`             | Version PublicId → QuoteVersionDetail                    | TS-005     | SC-Q02                 |
| POST   | `/api/crm/quotes/{quoteId}/reprice`                        | `repriceCrmQuote`                | Confirmed preview token + expectedRevision → new version | TS-005     | SC-Q02, SC-Q04         |
| POST   | `/api/crm/quotes/{quoteId}/copy`                           | `copyCrmQuote`                   | CopyQuoteRequest → new QuoteDetail                       | TS-005     | SC-Q05                 |
| POST   | `/api/crm/quotes/{quoteId}/transfer`                       | `transferCrmQuote`               | TransferQuoteRequest → QuoteDetail                       | TS-005     | SC-Q05                 |
| PUT    | `/api/crm/quotes/{quoteId}/archive`                        | `setCrmQuoteArchived`            | Archived boolean + reason → QuoteDetail                  | TS-005     | SC-Q05                 |
| POST   | `/api/crm/quotes/{quoteId}/ready`                          | `prepareCrmQuote`                | Pinned version + expectedRevision → QuoteDetail          | TS-010     | SC-D01, SC-Q07         |
| GET    | `/api/crm/custom-sewing`                                   | `getCrmCustomSewingDrafts`       | Scoped list query → paged CustomDraftSummary             | TS-006     | SC-S01                 |
| POST   | `/api/crm/custom-sewing`                                   | `createCrmCustomSewingDraft`     | CreateCustomDraftRequest → CustomDraftDetail             | TS-006     | SC-S01                 |
| GET    | `/api/crm/custom-sewing/{draftId}`                         | `getCrmCustomSewingDraft`        | PublicId → CustomDraftDetail                             | TS-006     | SC-S03                 |
| PUT    | `/api/crm/custom-sewing/{draftId}`                         | `saveCrmCustomSewingDraft`       | Custom configuration + revision → draft                  | TS-006     | SC-S01, SC-S03         |
| POST   | `/api/crm/custom-sewing/{draftId}/calculations`            | `calculateCrmCustomSewing`       | Selections/revision → price basis/blockers               | TS-006     | SC-S01, SC-S02         |
| POST   | `/api/crm/custom-sewing/{draftId}/versions`                | `saveCrmCustomSewingVersion`     | basis token/revision → immutable version                 | TS-006     | SC-S03                 |
| GET    | `/api/crm/custom-sewing/suggestions`                       | `getCrmSewingSuggestions`        | Type/season → IDs/norms/confidence                       | TS-008     | SC-S04                 |
| GET    | `/api/crm/quote-templates`                                 | `getCrmQuoteTemplates`           | List query → paged TemplateSummary                       | TS-008     | SC-S04                 |
| POST   | `/api/crm/quote-templates`                                 | `createCrmQuoteTemplate`         | Selections/introduction → TemplateDetail                 | TS-008     | SC-S04                 |
| PUT    | `/api/crm/quote-templates/{templateId}`                    | `reviseCrmQuoteTemplate`         | Template/revision → new TemplateVersion                  | TS-008     | SC-S04                 |
| POST   | `/api/crm/quote-templates/{templateId}/apply`              | `applyCrmQuoteTemplate`          | Version/target kind → new quote or private custom draft  | TS-008     | SC-S04                 |
| POST   | `/api/crm/files`                                           | `uploadCrmPrivateFile`           | Multipart file + owner/requestId → validated PrivateFile | TS-007     | SC-S05                 |
| GET    | `/api/crm/files/{fileId}/content`                          | `getCrmPrivateFileContent`       | Scoped file PublicId → private bytes                     | TS-007     | SC-S05, SC-D04         |
| GET    | `/api/crm/issuer-settings`                                 | `getCrmIssuerSettings`           | Approved active version → IssuerDetail                   | TS-009     | SC-D01                 |
| POST   | `/api/crm/issuer-settings/versions`                        | `createCrmIssuerSettingsVersion` | Verified settings → version                              | TS-009     | SC-D01                 |
| PUT    | `/api/crm/issuer-settings/active`                          | `activateCrmIssuerSettings`      | Version/revision → active settings                       | TS-009     | SC-D01                 |
| POST   | `/api/crm/quotes/{quoteId}/documents`                      | `renderCrmQuoteDocument`         | Version/template/render options → artifact/status        | TS-010     | SC-D02, SC-D04         |
| GET    | `/api/crm/quotes/{quoteId}/documents/{artifactId}`         | `getCrmQuoteDocument`            | Scoped artifact → status/metadata                        | TS-010     | SC-D02                 |
| GET    | `/api/crm/quotes/{quoteId}/documents/{artifactId}/content` | `downloadCrmQuoteDocument`       | Scoped artifact → private application/pdf bytes          | TS-010     | SC-D02, SC-D04         |
| POST   | `/api/crm/quotes/{quoteId}/deliveries`                     | `enqueueCrmQuoteDelivery`        | Version/PDF/explicit recipient → DeliveryDetail          | TS-011     | SC-D03                 |
| GET    | `/api/crm/quotes/{quoteId}/deliveries`                     | `getCrmQuoteDeliveries`          | List query → paged DeliveryDetail                        | TS-011     | SC-D03                 |
| POST   | `/api/crm/quotes/{quoteId}/production-packages`            | `exportCrmProductionPackage`     | Pinned version/selected variants → Package artifact      | TS-012     | SC-S06                 |

## Template lifecycle and introduction extension

TS-008/TS-016 also reserve `PUT /api/crm/quote-templates/{templateId}/archive`, operationId `setCrmQuoteTemplateArchived`: requestId, expectedRevision and archive boolean → TemplateDetail. Manager owns selection/configuration templates; Admin owns global issuer/document-layout settings. `applyCrmQuoteTemplate` returns a typed target (new quote draft for firstContact, custom draft for customConfiguration), with current server pricing only on explicit calculation. TS-016 freezes the layout/slots/email-template discriminators and first-contact public rendering parameters before frontend consumption. No independent first-contact mail/quote store.

## DTO decisions to encode in each slice's OpenAPI

Customer write: requestId, display/legal names, kind, optional tax ID, owner only where permitted, active flag; revise includes expectedRevision. Contact write: name/position/channels/primary/active and customer revision. Merge: survivor/victim IDs and both revisions, explicit reason. No source credentials or unrestricted role arrays.

Quote create/save: customer/contact PublicIds, public/private text, ordered item/variant intent, source refs, visibility, pricing method/reason and server basis tokens; requestId/expectedRevision. Actor, unit cost, coefficient, line/tier totals, sent timestamp and immutable number are server-owned and rejected as authoritative write fields. A migrated historical snapshot is accepted only by the isolated import pipeline, never this user endpoint. Typed discriminated item schemas reject mismatched kind/ref and unknown properties per existing conventions. One-off PPE can submit its explicit sale amount/reason; this exception is typed, not arbitrary calculated totals.

Detail reads return public IDs, revision, state/archive flags, current/issued version refs, typed content, calculation source summary/blockers, permitted action capabilities and scoped history links. Internal costs are returned only to authorized internal actors in internal DTOs; customer/PDF projection is a separate type. No unbounded nested version history on a list. Preview is a server write of a calculation basis, so it also requires requestId. Save cannot reuse another actor's basis.

List requests use existing page/pageSize conventions, capped pageSize (proposed 100, freeze in EN-001), bounded search/date/status/owner/archive filters and stable sorting with ID tie-breaker. Responses use `PagedResult<IReadOnlyList<T>>`: retain **value + pagedInfo**, including page number, size, total pages/count and navigation flags as existing serialization defines. List filters and count have identical access predicates. Do not invent a competing pagination envelope.

## Transport results, authentication and retries

Use current cookie/session auth, CSRF protections, Ardalis.Result and established HTTP/result mapping from the [integration guide](../../../../contracts/frontend-integration.md). Freeze exact wire fields and examples in canonical OpenAPI before client generation. Planned expected outcomes: 200 reads/updates, 201 created resources, 202 queued/rendering work; 400 malformed request, 401 unauthenticated, 403 forbidden action, 404 absent/inaccessible resource, 409 revision/idempotency/source-basis conflicts, 422 business/price/visibility blocker, 413 oversized file and 415 unsupported media. Do not expose stack traces, supplier secrets or foreign resource existence. If existing middleware maps a code differently, document and align it before freeze instead of publishing two meanings.

Mutation replay and revisions follow [consistency design](../design/security-and-consistency.md). Domain retries are explicit requestId replays, not blind transport retry with a new key. Reads may cancel/retry with existing client policy; aborted browser requests do not guarantee a server write was rolled back. PDF downloads are private and should not be cached publicly. Delivery POST returns queued status, not a false success/sent promise.

## Cross-repository handoff

P3 transfer UI also uses `getCrmQuoteAssignees`, a bounded eligible-user search returning only IDs and display names. The Host Identity adapter excludes locked users and non-Manager/Admin roles and revalidates eligibility on transfer.

TS-005 local implementation (2026-10-28) extends canonical `crm/quote-drafts.openapi.yaml` with getCrmQuoteVersions, getCrmQuoteVersion, getCrmQuoteEvents, repriceCrmQuote, copyCrmQuote, transferCrmQuote and setCrmQuoteArchived. The aggregate references each path. List adds createdFrom (inclusive), createdBefore (exclusive), ownerId and sourceQuoteId filters; timestamps normalize to UTC before SQL. History is paged with the existing envelope, actor/action/parent data; version detail retains typed saved content. Reprice accepts current versionId, expectedRevision and exactly one fresh scoped basis token for every variant; it does not accept edited content or browser prices. Existing per-variant preview supplies old/new comparison inputs. Source drift, expiry, missing token, changed revision or blockers abort confirmation without a version. Identical accessible writes replay; after transfer a former Manager's replay is 404 under current-scope authorization. Copy response has sourceQuoteId/sourceVersionId and a null number until EN-005.

After each implemented API slice passes focused tests, backend must provide a **clean immutable commit** with matching canonical OpenAPI. Frontend records full SHA/source path, runs existing sync/generate/check tooling and implements only pinned operations. The old snapshot does not satisfy this prerequisite. Commit authorization must follow the user's current instruction; this SDD does not itself authorize a future commit/push. No handwritten generated DTOs or canonical contract for unimplemented operations in this change.
