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
