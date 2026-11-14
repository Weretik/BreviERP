# Background jobs and runbooks

BreviERP has no in-process scheduler, queue or distributed lock. Delayed retries of side effects go through the [transactional outbox](outbox.md): Cloud Scheduler calls `POST /internal/outbox/dispatch` every minute (`brevierp-outbox`). Other periodic executions are the [Telegram reminders](telegram-reminders.md) daily run (Cloud Scheduler executes the `brevierp-reminders` job, the seed image with `--reminders-dispatch=true`) and the [sync job](sync-job.md) (`brevierp-sync`, `--sync=true`, Mon–Fri 09:00 Kyiv, not nightly: external refreshes and imports one step after another, now the KeepinCRM suppliers and clients refresh). `Host.Api` calls `AddBackgroundJobRegistrations()`, but that method currently registers no services.

One-off operational jobs exist as libraries in `src/Jobs` ([overview](../../../../src/Jobs/README.md)): `CatalogImport` (catalogue import and PPE pricing bootstrap) and `SewingPricing` (pricing baseline, verification, readiness, input review, and activation). Only `Host.Seed` references and runs them, through command-line flags. Runbooks with exact commands:

- [Catalogue import](../data/catalog-import.md) and [PPE pricing bootstrap](../data/ppe-pricing.md)
- [Sewing pricing baseline](../data/sewing-pricing.md), [readiness](../data/pricing-readiness.md), [input review](../data/pricing-input-review.md), [activation](../data/pricing-activation.md), [cutover verification](../data/pricing-cutover.md)

```text
operator / cron / CI → agreed job implementation → scoped module service → PostgreSQL / external adapter
```

- [Outbox dispatch](outbox.md)
- [Job CLI status](host-jobs-cli.md)
- [Troubleshooting](troubleshooting.md)

For a new background operation, create an SDD first. It must define the start command, contract, idempotency, timeout and retry behavior, locking, observability, safe rerun, and rollback. Never put secrets or real endpoint URLs in output, tracker tasks, or documentation.

## Jobs audit 2026-11-14

Cloud Run Jobs: `brevierp-seed` (deploy), `brevierp-reminders` and `brevierp-sync` (both Mon–Fri 09:00 Kyiv), and the one-off catalogue import job that the deploy workflow reconfigures. Cloud Scheduler: reminders, sync, and the planned outbox job. Proposals waiting for the owner, no cloud change made: run reminders as the first `brevierp-sync` step and remove `brevierp-reminders` with its scheduler entry (two scheduler jobs of the three free ones with the outbox); remove the catalogue import job and its deploy step once the import is finished. The job steps in `Host.Seed/Jobs` move to their modules in stage 8 ([module rules](../../standards/module-rules.md#known-deviations)). Scripts the team runs by hand are in `tools/` (`tools/catalog-import`, `tools/cloudflare/api-proxy`).
