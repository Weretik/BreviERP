# ADR-0009: Side effects leave through a transactional outbox per module

**Status:** accepted · **Date:** 2026-11-15 (recorded from the code; [PLT-003](../../specs/platform/003-transactional-outbox/README.md)) · **Owner:** Виталий (project owner)

## Context

Several use cases change data and must tell the outside world: a site request goes to the Telegram group, a production order is posted to Telegram, a change of pricing settings must reach other modules. Sending before the commit can report data that was never saved; sending after can be lost by a crash. Cloud Run scales to zero, so an in-process retry loop is not reliable.

## Options

- Send inside the request: rejected; failures are lost.
- Pub/Sub or a broker with MassTransit: rejected; too much infrastructure for a few messages a day.
- Hangfire or a hosted service: rejected; Cloud Run stops idle instances.
- An outbox table per module, dispatched by a scheduled call: chosen.

## Decision

- A use case writes an `OutboxMessages` row in its module's schema in the same `SaveChanges` as its data.
- After the commit the request tries the row once; the rest is retried with growing delays (up to 60 minutes, ten attempts).
- The table, writer and dispatcher are shared code in `BuildingBlocks.Infrastructure`; each module registers its own idempotent handlers (`AddOutboxHandler<THandler>`), for example Crm `crm.site-request.notify`, Production order to Telegram, Reference pricing settings changed.
- Retries run inside the scheduler tick ([ADR-0010](0010-scheduler-tick-for-background-tasks.md)); the older `POST /internal/outbox/dispatch` endpoint stays for a direct call.

## Consequences

- Data and its message commit together.
- A message can arrive late and more than once; handlers ignore repeats.
- Rows that ran out of attempts stay with `LastError` and must be watched ([outbox runbook](../../operations/jobs/outbox.md)).

## Sources

`src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure/Outbox/`, [module rules: transactional outbox](../../standards/module-rules.md#transactional-outbox).
