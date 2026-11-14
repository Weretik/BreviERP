# PLT-003 — Transactional outbox

**Status:** implemented 2026-11-14; Cloud Scheduler job (EN-01) pending. **Date:** 2026-11-14. **Module:** platform (`BuildingBlocks`, `Host.Api`); first consumers Crm, Production, Reference → Catalog. **Rule:** [module rules «Transactional outbox»](../../../standards/module-rules.md#transactional-outbox).

## Why

Three use cases save data and then call the outside world, or another module, without a retry:

| Use case | Today | What goes wrong |
| --- | --- | --- |
| Site request (`SubmitSiteRequest`, LEAD-002) | Saves the request, posts it to Telegram, saves the result in a second `SaveChanges` | A Telegram outage or a restart after the first save leaves the request without a message; nobody retries it |
| «Надіслати в цех» (`SendProductionOrderTelegram`) | Saves «Надсилається», sends, saves the result | A restart between the two saves leaves the order «Надсилається» forever: the button refuses to send again |
| «Параметри ціноутворення» (Reference) → Catalog | After the Reference save, calls `IPricingSettingsSync` in the same request; a failure is only logged | Catalog keeps the old numbers until the next change; nobody sees it |

## Decision

One shared outbox: each module keeps an `OutboxMessages` table in its own schema, written in the same `SaveChanges` as the change. After the commit the request tries its own rows once. A protected endpoint `POST /internal/outbox/dispatch`, called every minute by one Cloud Scheduler job, retries what is left. No message bus, no hosted timer, no Hangfire (Cloud Run scales to zero).

Not in scope, with the reason:

- `SendQuote` (Gmail): the manager waits for the result and sees a Gmail refusal at once; the outbox would hide it.
- Media and site request file uploads to the CDN: the caller needs the URL in the same request.
- Telegram test messages, webhook connect, mailbox connect: the result is the answer.
- Reminder dispatch, `brevierp-sync` steps, sheet and KeepinCRM syncs: scheduled runs that re-read their source; a failed run is repeated by the next one.

## Rules

- **R-01 Same transaction.** A use case with an outside side effect adds an `OutboxMessage` (id, type, JSON payload, created at) to its module's `DbContext` in the same transaction as its change: one `SaveChanges` where it can; a row whose id comes from the database (site request) is saved first inside an explicit transaction. A rolled-back change leaves no message.
- **R-02 Try at once.** After a successful save the handler asks the dispatcher to process the ids it just wrote, before it answers. The answer does not wait for a retry and does not fail when the try fails.
- **R-03 Retry.** The dispatcher takes due rows (`ProcessedAt is null and NextAttemptAt <= now and Attempts < 10`) with `FOR UPDATE SKIP LOCKED`, at most 20 per module per call, oldest first. A failure stores the error (≤ 2000 characters), adds one to `Attempts` and moves `NextAttemptAt` by 1, 2, 4 … minutes, at most 60.
- **R-04 Give up visibly.** On the tenth failure the row stays with its last error and the dispatcher logs an error (`Outbox message {Type} {Id} failed 10 times`). The consumer's own record shows it too (R-07, R-08).
- **R-05 Idempotent handlers.** A message may be handled twice (a restart between the side effect and the mark). Every handler checks its own record first and does nothing when the work is already done. A handler answers «done» or «try later» with a reason; an exception counts as «try later».
- **R-06 Protected endpoint.** `POST /internal/outbox/dispatch` accepts only a Google OIDC token whose audience is `Outbox:Audience` and whose e-mail is `Outbox:SchedulerServiceAccount`; anything else gets 401 with no body. Without that configuration the endpoint answers 404 and only the immediate try runs. The answer is `{ processed, failed }`.
- **R-07 Site request.** `SubmitSiteRequest` saves the request and a `crm.site-request.notify` message (`{ requestId }`) together and answers 200 as today. The handler builds the message from the saved request, posts it, and records `Notified` with the time or the reason; a request already notified is skipped. When no group got it (Telegram down, no group ticked, bot not set) it keeps the reason on the request and answers «try later», so the request is posted once Telegram or the setting is back.
- **R-08 Workshop Telegram.** «Надіслати в цех» saves «Надсилається» and a `production.order.telegram` message (`{ orderId }`) together; the handler sends only while the delivery is «Надсилається» and marks it sent or failed. A Telegram refusal is «done» for the outbox: the delivery is «Не надіслано» with the reason and the person presses again, as today (they asked for it and see the answer). The outbox covers a stop between the two saves. The answer shows the delivery as it is after the immediate try.
- **R-09 Pricing settings.** A change of «Параметри ціноутворення» or a PPE markup rule saves a `reference.pricing-settings.changed` message (no payload) with it; the handler calls Catalog `IPricingSettingsSync`, which re-reads all parameters, so repeats are harmless, and then publishes the changed Sewing prices itself (the request's publication filter no longer sees it). The sync stops swallowing its failure: a refused or thrown sync is «try later»; a publication failure is only logged and waits for `brevierp-sync`, as before.

## Scenarios

- **SC-01** A visitor sends a request while Telegram works → 200, one message in the group, the row is processed.
- **SC-02** Telegram is down → 200, the request is saved with the reason, the row is due again in 1 minute; when Telegram is back the next dispatch posts it and clears the reason.
- **SC-03** The save fails → no request and no message.
- **SC-04** The same row is processed twice → one Telegram message.
- **SC-05** The process stops after «Надсилається» is saved → the next dispatch sends the order; the button is not stuck.
- **SC-06** A pricing parameter changes and the Catalog sync throws → the parameter is saved, the row retries, Catalog gets the numbers on the next try.
- **SC-07** A call to the dispatch endpoint without a token, with a wrong audience or a wrong e-mail → 401; with the scheduler's token and nothing due → 200 `{ processed: 0, failed: 0 }` without other queries.
- **SC-08** A row fails ten times → it stays with its error and an error is logged; it is not taken again.

## Design

| Layer | Path | Responsibility | State |
| --- | --- | --- | --- |
| Application | `BuildingBlocks.Application/Outbox/OutboxMessage.cs` | the row: `Create(type, payload, now)`, `Succeed`, `Fail(error, now)` with the backoff | done |
| Application | `BuildingBlocks.Application/Outbox/IOutboxHandler.cs` | `Type` and `HandleAsync(payload)` → `null` (done) or the reason to try later | done |
| Application | `BuildingBlocks.Application/Outbox/IOutboxDispatcher.cs` | `DispatchAsync(ids)` for the immediate try, `DispatchDueAsync()` for the endpoint | done |
| Infrastructure | `BuildingBlocks.Infrastructure/Outbox/OutboxModelBuilderExtensions.cs` | `modelBuilder.AddOutbox()`: table `OutboxMessages` in the context's schema, index on (`ProcessedAt`, `NextAttemptAt`) | done |
| Infrastructure | `BuildingBlocks.Infrastructure/Outbox/OutboxDispatcher.cs` | per `DbContext`: claim with `SKIP LOCKED`, run the handler in its own scope, save the result | done |
| Infrastructure | `BuildingBlocks.Infrastructure/Outbox/OutboxServiceCollectionExtensions.cs` | `AddOutbox<TContext>()` and `AddOutboxHandler<THandler>()` | done |
| Api | `BuildingBlocks.Api/Outbox/OutboxDispatchEndpoint.cs` | `MapOutboxDispatch()`, OIDC check (`Google.Apis.Auth`), runs every registered dispatcher | done |
| Host | `Host.Api` | maps the endpoint, binds `Outbox` options | done |
| Crm | `SiteRequests/Notify/NotifySiteRequestOutboxHandler.cs`; `Submit/` writes the message; migration `AddOutbox` | R-07 | done |
| Production | `Orders/Telegram/SendProductionOrderTelegramOutboxHandler.cs`; command writes the message; migration `AddOutbox` | R-08 | done |
| Reference | `PricingSettingsOutboxInterceptor` adds the message to every save of a pricing parameter or markup rule, so the seven handlers stay as they are; `PricingSettingsChangeNotifier` tries it at once; `PricingSettingsChangedOutboxHandler` calls Catalog `IPricingSettingsSync`; migration `AddOutbox` | R-09 | done |

Data: one new table per consumer schema (`crm`, `production`, `reference`): `Id uuid`, `Type varchar(100)`, `Payload jsonb`, `CreatedAt`, `Attempts int`, `NextAttemptAt`, `ProcessedAt null`, `LastError varchar(2000) null`. No change to existing tables. Rollback: the code is reverted; the tables stay and are ignored.

## Prerequisites

- **EN-01 Cloud Scheduler job** (owner, through the coordinator): service account `brevierp-outbox`, job `brevierp-outbox` every minute, HTTP POST to `https://<Cloud Run URL>/internal/outbox/dispatch`, OIDC token with that audience; Cloud Run settings `Outbox__Audience` and `Outbox__SchedulerServiceAccount`. It is the third job (with `brevierp-reminders` and `brevierp-sync`), within the three free ones. Until it exists only the immediate try runs, which is no worse than today. Runbook: [outbox dispatch](../../../operations/jobs/outbox.md).

## Tasks and evidence

- **TS-01** Shared outbox and endpoint: `OutboxMessageTests` (backoff, limit, error length), `OutboxDispatcherTests` (PostgreSQL: same transaction, claim once, retry, give up), `OutboxDispatchEndpointTests` (401 cases, 404 without configuration, 200).
- **TS-02** Site request (R-07): `SubmitSiteRequest` tests for SC-01..SC-04.
- **TS-03** Workshop Telegram (R-08): SC-05 and the existing send tests.
- **TS-04** Pricing settings (R-09): SC-06.
- **TS-05** Runbook, module rules «BreviERP today», feature map, known deviation row removed.
