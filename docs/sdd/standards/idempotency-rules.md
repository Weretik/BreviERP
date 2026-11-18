# Idempotency rules

An operation is **idempotent** when doing it twice with the same input leaves the same result as doing it once: no second row, no second payment, no second stock movement, no second message. Repeats are normal, not rare: a person presses twice, a phone loses the answer and retries, a proxy times out after the server committed, and every background mechanism here (outbox, scheduler, webhooks, sync jobs) delivers **at least once**. This document says where protection is required, which pattern to use, and how to check it. It extends [database rules](database-rules.md) section 4 (concurrency) and the outbox rule in [module rules](module-rules.md).

## 1. Where protection is required

Decide for every write operation in the SDD design (next to each command or endpoint: what a repeat does and which protection) and record it in the contract description.

| Operation | What a repeat must do | Protection |
| --- | --- | --- |
| Create of money, stock, orders, numbered documents, payroll, anything that leaves the system (Telegram, e-mail, 1C, SMS) | Return the first result; add nothing | **Request key** (section 2) — required |
| Public anonymous form (site order, site request, call-back) | Return the first result; the managers get one message | **Request key** — required; rate limiting is not a substitute |
| Send from an offline or mobile queue | Same as above | **Request key** stored with the queue entry |
| Delta on shared data (add meters to stock, +1 counter, append a ledger line) | Apply once | **Request key** or an operation id on the ledger row with a unique index |
| State transition (send, close, cancel, retry, publish) | Answer from the current state; no second side effect | **State check** (section 3): the transition is a no-op when the target state is already reached; a transition the state forbids answers 409 |
| Replace or set (PUT a value, set a status, set a list) | Same final state | Naturally idempotent: write the target value, never a delta; concurrent edits use a row version ([database rules](database-rules.md) 4) |
| Delete | Nothing is left | Deleting what is already gone answers the same success (or the same 404) every time, never 500 |
| Create of reference data with a natural unique key (code, slug, e-mail, name in a list) | Second copy refused clearly | Unique index + caught violation → 409 with a Ukrainian message; no request key needed |
| Create of a plain record without a natural key (task, note, FAQ item, folder) where a duplicate is only a nuisance | — | The client lock while pending is enough (frontend rules); add a request key when a duplicate costs money or confuses people |
| Reads, searches, previews, calculations without stored effects (also `POST` used for a complex query) | — | Nothing |
| Login, token refresh, logout | — | No request key: refresh rotation keeps a short grace window for a parallel or repeated refresh |

**Rule of thumb for agents:** if repeating the request would create something, move something, or tell someone something a second time, it needs a request key or a state check. If it only overwrites with the same value, it is already idempotent.

## 2. Request key (client-made idempotency key)

- **Who makes it.** The client, once per attempt: when the form opens or on the first submit of a payload. It reuses the same key for every retry of that attempt (timeout, network error, unknown outcome, second press) and makes a new one after a success or when the person changes the data. Frontend details are in the frontend project's rules.
- **Wire format.** New endpoints take the `Idempotency-Key` header with a UUID. A project that already uses a body field (`requestId`) keeps it; record the choice under project specifics (BreviERP: the body field, see below). The key is optional on the wire for compatibility, but every own client sends it.
- **Storage.** Saved in the **same `SaveChanges`** as the result, under a **unique index**:
  - a `RequestId` column on the created row with a unique partial index (`HasFilter("\"RequestId\" IS NOT NULL")`) when the result is one row;
  - a separate idempotency record (`Operation`, `Key`, `RequestHash`, result id, `CreatedAtUtc`, `ExpiresAtUtc`) with a unique index on (`Operation`, `Key`) when the result spans rows, the key must expire, or the same key must be checked against the request data;
  - the created row's id itself (the key becomes the id, so the primary key is the guard) when the client may choose the id.
- **Flow in the handler.**
  1. Look the key up. Found and the request data is the same → answer with the saved result (200, same body as the first answer). Found with other data → 409 `idempotency_conflict` with a Ukrainian message.
  2. Otherwise do the work and save the key with the result in one transaction. Side effects go through the outbox, so they are written once with the result.
  3. Catch the unique violation on the key (`UniqueViolation.Is` / `DbUpdateException`): another copy won the race. Clear the tracked changes, read the winner, and answer as in step 1. Never answer 500 to a repeat.
- **Comparing data.** Compare the business fields of the request (or a hash of their canonical JSON), never timestamps or the key itself.
- **Scope and lifetime.** A UUID is unique on its own; scope it by actor or operation when keys come from several clients. Keep keys at least as long as a client may retry (hours for forms, days for offline queues) and remove expired ones in a scheduled task.
- **Actions with a version.** A command that already sends the row version (`Version`, `xmin`) is protected against a blind second write: the repeat gets 409 "stale". When the first call has an external effect (an e-mail, a Telegram message), a request key is still needed so the repeat answers success instead of a confusing conflict.

## 3. Handlers that run at least once

Every outbox handler, webhook handler, scheduled task, sync job, and import must be safe to run twice with the same input.

- **State check first.** The handler reads the target state and returns when the work is done (`NotifiedAt` is set, the delivery is no longer `Sending`, the order is already accepted by 1C). The state change is saved in the same transaction as the outbox row is marked processed.
- **Dedupe key at the receiver.** A handler that writes rows stores the message id, the external event id (Telegram `update_id`, a webhook event id, a bank statement line fingerprint), or the source's natural key under a unique index and skips a known one.
- **Upsert, not insert.** Sync and import jobs match by the source's natural key (1C code, external id, file fingerprint) and update or insert; a rerun of the same file or period changes nothing.
- **External systems without a dedupe key** (Telegram send, e-mail): mark the work done in the same transaction that completes the outbox row. A crash between the send and the commit can still produce one duplicate; accept it and write it down, but never resend once marked. When the external system accepts a key or a number (1C document number, a message reference), send it so the other side can drop the repeat.
- **Webhooks** answer 2xx for an event they have already handled, so the sender stops retrying; they verify the secret first.
- **Scheduled tasks** run one at a time (advisory lock or scheduler group, [database rules](database-rules.md) 4) and a rerun for the same period skips the work already done (a period marker or a per-item state).
- **Long jobs** are split into steps; each step is one transaction and can be rerun ([database rules](database-rules.md) 3.3).

## 4. Tests

Write them first (Red → Green), next to the use case's other tests:

- Unit: a repeat with the same key returns the first result and saves and sends nothing; the same key with other data answers 409; without a key two requests are two results; a copy that loses the race answers with the winner.
- Integration on real PostgreSQL: two parallel requests with one key → one row and both 2xx; a handler run twice with one message → one side effect.
- Contract: the header or field and the 409 answer are in the OpenAPI document.

## 5. Check (audits)

- Every write endpoint from section 1 that needs a request key has one, stored under a unique index in the same transaction as the result.
- The unique violation on the key is caught and answered with the saved result, never 500.
- A state transition with an external effect is a no-op when repeated.
- Every outbox handler, webhook, scheduled task, and import has a state check, a dedupe key, or an upsert; long jobs are rerunnable steps.
- The contract documents the key and the 409; the client sends it (frontend audit).
- Known gaps are listed under «Known deviations» below with the risk and the plan.

## BreviERP specifics

- **Wire format:** an optional body field `requestId` (UUID) made when the form opens; the admin and the site renew it after a success. New endpoints follow the same field, not a header, so the whole API reads one way.
- **Storage:** a `RequestId` column on the created row with a unique partial index (`WHERE "RequestId" IS NOT NULL`). A repeat returns the saved row; a copy that loses the race catches the `DbUpdateException`, clears or detaches its changes, and reads the winner by `RequestId`. Beside it a `RequestHash` (SHA-256 of the request without the id, `RequestKeys.Of` in `BuildingBlocks.Application/Idempotency/`) is saved; a repeat whose hash differs answers 409 «Цей запит уже виконано з іншими даними» (`RequestKeys.Conflict`). Rows saved before the hash existed have none and answer with the first row.
- **Warehouse:** stock commands carry an `OperationId`; `StockOperations.ReplayAsync` stores it with a hash of the actor, kind, and input, answers a repeat with the saved result, and refuses the same id with other data.
- **Versions:** edits and order tracking send `Version`; a repeat after a success gets 409 «stale» and reloads (no double write).
- **Outbox:** each module's outbox table is read with `FOR UPDATE SKIP LOCKED`, has a handler timeout, and a partial index on unsent rows; handlers check state before acting (section 3).

### Audit 2026-11-15: write operations and handlers

| Operation or handler | What a repeat does | Verdict |
| --- | --- | --- |
| Finance operations and transfers, payroll payments and adjustments | The first row is returned; other data answers 409 | Protected (`RequestId` + `RequestHash`, database stage 4a) |
| Work log entries, КП create / introduction / copy, «Надіслати КП» e-mail, lead calls, site requests | The first row is returned; no second e-mail or Telegram post; other data answers 409 | Protected (`RequestId` + `RequestHash`, database stage 4b); the site form sends it |
| Production orders create and copy, customers create / contact / revise / merge, pricing versions and sewing calculations | The first row is returned; production orders, customers and pricing answer 409 for the same id with other data | Protected (`RequestId` + `RequestHash` or a saved request hash) |
| Fabric stock: receive, issue, adjust, transfer, issue to item, fabric issue | The saved result is replayed; the same id with other data is refused | Protected (`OperationId` + hash) |
| Shipments, close / reopen / cancel order, payroll month close / reopen, voids | A repeat gets 409 «stale» or answers from the current state | Protected (row `Version`, state checks) |
| Bank statement upload and booking | The same file is refused per account; booking re-checks duplicates by fingerprint and external id | Protected (unique `FileHash`, fingerprints) |
| Imports: workshop orders, work log, staff, archive, fabric norms, legacy finance, embroidery archive, purchase invoices | A repeat skips what was moved (legacy id, source uid, model code) or updates it | Protected (upsert by the source key) |
| Outbox: site request to Telegram, production order to Telegram, fabric history to Production, pricing settings to Catalog | Skipped when already notified / not `Sending` / the history line exists; the settings sync is a full replace | Protected (state check). A crash between a Telegram send and the commit can post once more: accepted risk |
| Outbox: `activity.recorded` to «Журнал дій» (ACT-001, all six source modules) | The entry id is the event id: a repeat finds the row and does nothing; a racing copy is refused by the primary key and counts as done | Protected (dedupe key) |
| Telegram webhook (reminder buttons, chat membership) | «Виконано» on a closed period answers «Вже закрито»; two presses race on the occurrence `Version` | Protected (state check + row version) |
| Daily reminders, scheduler tick | A period already sent today is skipped; one tick at a time under `pg_try_advisory_lock` | Protected |
| Admin creates without a natural key: finance accounts, categories, counterparties, workshop employees, file uploads | A double press could add a second item | Acceptable: the admin buttons are locked while the request runs and nothing leaves the system; counterparties can be merged |
| Updates and deletes of reference data | Same final state | Naturally idempotent (row versions where several people edit) |

## Known deviations

None open.

Remove a row when it is fixed.
