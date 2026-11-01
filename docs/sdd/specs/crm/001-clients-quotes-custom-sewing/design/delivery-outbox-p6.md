# P6 explicit delivery queue and Gmail OAuth

Scope: TS-011 / SC-D03 and the remaining code portion of EN-004. Existing user choices select Gmail/Google Workspace, the sender and a separate sandbox recipient. Addresses remain local configuration. No old credentials are imported. This revision refines existing accepted delivery behavior and does not activate sending or claim provider round-trip acceptance.

## Readiness boundary

The outbox code may be implemented and tested with fake provider/credentials while runtime sending stays disabled. Enqueue and worker dispatch require explicit Enabled configuration, the matching provider/sender identity, bounded timeout/lease, configured delegated OAuth and sandbox recipient policy. Structural credential presence is not proof of consent or delivery: the token source verifies the Gmail profile address before returning a token. EN-004 remains partial until actual consent and controlled provider round-trip; TS-011 production acceptance remains gated on it.

## Rules and scenarios

- R-D04.a / SC-D03 enqueue is an explicit Admin/current-owner action with requestId and expectedRevision, exact currently issued version, successful intact private PDF and immutable email-template version. Recipient and manager display name are explicit bounded public inputs. Saved recipient/number and template expansion supply text; callers cannot submit subject/body/PDF/pricing.
- R-D04.b: queue insertion, pinned content, actor/request intent receipt and audit event commit together. Current authorization precedes replay hash comparison; same actor/key/input replays one delivery, changed payload conflicts. Archived quotes reject new requests. A fresh requestId for the same version/recipient is rejected while an earlier delivery is queued, in flight or unknown; changing a key cannot bypass reconciliation. A new explicit action is allowed after a confirmed terminal result. Import history remains inert.
- R-D05.a: queue, attempt and unknown are distinct from sent. Worker commits an exclusive lease/attempt before any provider IO. Only one claimant submits; expired in-flight attempts become uncertain and use read-only reconciliation. An empty lookup never authorizes a resend. Confirmed failure is terminal for this slice; no retry endpoint is introduced.
- R-D04.c: immutable delivery pins include provider, sender, quote version, template, expanded text, recipient, original actor, PDF bytes/hash and time. Worker never silently changes account/template/document after configuration or master changes. Provider/sender mismatch blocks processing. Queued archived quotes are finalized without send; uncertain prior attempts may still be reconciled.
- R-D05.b: confirmed provider success records a version-specific immutable event and final state once. Current quote becomes sent only when the confirmed version is still its current issued version and state ready; a newer draft is never marked sent. Sent means provider acceptance, not customer receipt/read.
- EN-004.a: Gmail token refresh uses fixed Google endpoints, secure host configuration and bounded HTTP; no automatic send retry. Only a token for the configured sender profile is returned. Refresh concurrency/cache are bounded; errors and logs never contain credentials, bodies or raw provider responses.

## Planned ownership

```text
Crm.Domain/Delivery/                                  pure delivery state/identifiers
Crm.Domain/Quotes/Quote.cs                            guarded confirmed-version sent transition
Crm.Application/Delivery/EnqueueQuote/                command and handler
Crm.Application/Delivery/GetDeliveries/               safe bounded history contracts/use cases
Crm.Application/Delivery/Outbox/                      persistence/identity/claim ports
Crm.Application/Delivery/Processing/                  one-operation processor
Crm.Infrastructure/Persistence/Delivery/              pinned source, queue/read/claim/finalize transactions
Crm.Infrastructure/Persistence/Configurations/Delivery/  constraints and indexes
Crm.Infrastructure/Delivery/Gmail/OAuth/              delegated refresh/profile verification/cache
Crm.Api/Delivery/                                    explicit queue and authorized history HTTP
Host.Api/Crm/Delivery/                               bounded hosted polling and scope ownership
```

The coordinator owns shared docs, aggregate contract, DI/host integration and final checks. Outbox contributor owns all new schema/migration edits; OAuth contributor does not edit shared model. No commits or remote publication are authorized by implementation delegation.

## API and data

Canonical module contract: `docs/sdd/contracts/crm/quote-deliveries.openapi.yaml`. Route `/api/crm/quotes/{quoteId}/deliveries`: POST `enqueueCrmQuoteDelivery`202; GET `getCrmQuoteDeliveries`200 paged; `/{deliveryId}` GET `getCrmQuoteDelivery`200. Errors400/401/403/404/409 and unavailable readiness503 follow existing Result mapping. Body carries requestId, expectedRevision, versionId, artifactId, emailTemplateVersionId, recipientAddress and managerName only; unknown fields are rejected. Reads return bounded statuses/events and opaque IDs, not body/PDF/secret/token.

Crm owns immutable delivery pins, mutable processing state, append-only events and request receipts. Internal relational keys use long (typed Domain identifiers); API IDs use separate UUIDs. Same-quote version/PDF FKs and unique actor/request key protect scope. State lease tokens prevent stale finalization. Quote is row-locked for enqueue and confirmed success; worker claim uses skip-locked rows and provider IO is outside transactions.

Migration is additive after EmailTemplateRegistry and ProductionPackages. Verify disposable fresh/reapply and local upgrade after target check/backup through CRM-only Host.Seed. Rollback after use must retain delivered/history evidence: disable dispatch and recover forward or restore coordinated backup; destructive Down is only for unused isolated schema.

## Verification and acceptance

Focused fake HTTP OAuth tests cover missing config, success/profile match, cache/concurrent refresh, invalid scopes/expiry/malformed/failure and account mismatch. Outbox tests cover permissions/replay, strict contract, pinned sources, duplicate claim, expired lease/unknown, late finalizer, success event/current-vs-newer version, archive and identity mismatch. Dedicated unit tests cover state transitions. Use unique local PostgreSQL test databases. No real email or interactive consent is part of automatic tests.

Frontend sync still requires a separately authorized immutable backend commit; this slice does not modify frontend. EN-004 remains explicitly partial if real consent/round-trip is unavailable.

## Send-only revision — 2026-11-01 (supersedes lookup requirements above)

Owner-selected scope: Gmail send only with manual inspection of uncertain outcomes. EN-004 and TS-011 implement this revision before live consent. OAuth requests gmail.send, openid and email; sender verification uses the fixed Google OpenID Connect UserInfo endpoint with a nonempty subject, matching email and email_verified=true. No Gmail profile, list, search or message-read request is allowed. Explicit scope responses must contain the required send/identity permissions; the email permission may use Google's userinfo.email alias. Missing refresh-response scope retains the prior grant and still requires identity verification.

A send-only provider advertises no lookup capability. Readiness retains the stable Message-ID and all existing credential/sandbox/lease checks but does not require mail-reading capabilities. Uncertain sends and expired send leases retain unknown, never imply success and never permit an automatic resend. Unknown completion clears NextCheckAt rather than scheduling background reconciliation; legacy scheduled unknowns are consumed once into manual review without external IO. Existing generic lookup-capable ports and historic events remain compatible. No schema, HTTP operation or destructive migration is added.

SC-D03 send-only checks: successful refresh verifies the configured verified identity without reading mail; missing/unverified/mismatched identity blocks; consent requests only send and identity scopes; unknown Gmail transport performs one POST and no lookup; expired send-only claims become manual unknown with no token or provider request; persisted unknown is not reclaimed after time advances and blocks new-key duplicate sends. Manual ERP outcome-resolution actions and actual Google consent/test send are separate remaining tasks; this slice does not invent a confirmed result or enable dispatch.

Sources: [Google OpenID reference](https://developers.google.com/identity/openid-connect/reference), [Gmail send method scopes](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/send).

## Manual outcome resolution — 2026-11-01

TS-011 / SC-D03: current owner Manager or Admin explicitly records a checked result for unknown or expired sending/reconciling attempts. Active leases and terminal outcomes reject changes. Request includes requestId, expectedAttempt, outcome sent/not_sent and required bounded plain-text evidenceNote (1–1000 trimmed chars, no NUL/control except newline/tab). Actor/time are server-owned. An absent Gmail Sent entry is not treated automatically as proof of failure. Manual resolution never calls Gmail, changes recipients/content or resends.

POST /api/crm/quotes/{quoteId}/deliveries/{deliveryId}/resolution, operation resolveCrmQuoteDelivery, returns safe delivery200, invalid400, unauthorized401/403, inaccessible404, conflict409. Existing staff/current ownership checks precede replay hashes. Same actor/key/normalized input replays; changed intent conflicts, including collision with enqueue keys. Exactly one immutable resolution per delivery. State and quote locks follow completion order (state then quote); ownership is rechecked under quote lock. Manual success marks sent only for the matching current issued ready version; newer drafts remain untouched. Manual not_sent leaves the quote's current state unchanged and permits a later explicit enqueue. Late worker finalizers cannot overwrite a resolution.

crm owns QuoteDeliveryResolutions keyed by existing typed long DeliveryId (one-to-one), same-quote FK, actual actor, attempt, outcome, note and UTC time. Append-only trigger reuses existing protection function. Existing request receipts protect idempotency with an operation-tagged intent hash; manual outcome events use actual resolver actor. Additive migration introduces this table and extends the event-kind constraint, without editing applied migrations. No new public database ID is needed: the API addresses the existing delivery PublicId. Existing DTO gains optional nullable manualResolution metadata and canResolveManually flag. Old rows/clients remain readable; deploy migration/backend before the new UI and expose manual writes only through updated clients. UI labels manual confirmation separately from provider acceptance.

Verification: Domain unknown/expired/active/terminal/late-finalizer tests; PostgreSQL idempotency/changed payload, permissions after transfer, parallel conflicting resolutions, current-vs-newer version, manual-failure explicit enqueue, immutable record and fresh/reapplied migration. Strict JSON/FluentValidation/API/contract tests. Frontend dialog captures same request on unknown, expected attempt conflicts preserve input, history displays author/time/note, old API rows default safely. No provider credentials required for manual resolution.

Rollback: disable manual UI/actions before an older binary; retain resolution evidence and recover forward after real use. Down is only for isolated unused test databases. Local application upgrade requires verified target and backup; isolated fixture migrations are permitted for verification. EN-004 actual send is blocked on absent OAuth client JSON/token per owner reply; do not run a worker against the application DB or manufacture a sent result.
