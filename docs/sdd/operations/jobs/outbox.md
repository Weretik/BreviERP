# Outbox dispatch runbook

Specification: [PLT-003 transactional outbox](../../specs/platform/003-transactional-outbox/README.md). Rule: [module rules «Transactional outbox»](../../standards/module-rules.md#transactional-outbox). Do not put real URLs, tokens or service account keys into documentation or logs.

## What runs

A use case with an outside side effect saves an `OutboxMessages` row in its module's schema with its change, then tries it once before it answers. What is left is retried by `POST /internal/outbox/dispatch` on `Host.Api`, called every minute by one Cloud Scheduler job. Message types today:

| Type | Module | Work | A failure |
| --- | --- | --- | --- |
| `crm.site-request.notify` | Crm (`crm.OutboxMessages`) | Posts a site request to the bot's groups | The reason stays on the request («Заявки з сайту»); retried |
| `production.order.telegram` | Production | Sends «Надіслати в цех» while the delivery is «Надсилається» | A Telegram refusal is shown on the order and the person presses again; only a stop between the saves is retried |
| `reference.pricing-settings.changed` | Reference | Hands «Параметри ціноутворення» and PPE markup rules to Catalog, then publishes the Sewing prices | Retried; a publication failure waits for `brevierp-sync` |

Retries: 1, 2, 4 … minutes, at most 60, ten attempts. After the tenth failure the row stays with `LastError` and the API logs `Outbox message {Type} {Id} failed 10 times`.

## Configuration

| Setting | Where | Secret |
| --- | --- | --- |
| `Outbox__Audience` | API environment variable: the audience the scheduler puts in its OIDC token (the public API origin + `/internal/outbox/dispatch`) | No |
| `Outbox__SchedulerServiceAccount` | API environment variable: e-mail of the `brevierp-outbox` service account | No |

Without both settings the endpoint answers 404 and only the immediate try runs. The deploy workflow uses `--update-env-vars`, so values set once on the service stay across deploys.

## Set up once (cloud change: through the coordinator)

1. Create the service account `brevierp-outbox` (no roles needed: the endpoint checks its e-mail in the token, Cloud Run itself is public behind the Worker).
2. Set `Outbox__Audience` and `Outbox__SchedulerServiceAccount` on the API service.
3. Create the Cloud Scheduler job `brevierp-outbox`: schedule `* * * * *`, time zone `Europe/Kyiv`, HTTP POST to `<Cloud Run URL>/internal/outbox/dispatch`, no body, auth «OIDC token» of `brevierp-outbox` with the audience from step 2. Use the Cloud Run URL, not the Worker domain.
4. Check: the job's last run is 200 and its answer is `{"processed":0,"failed":0}` when nothing is due.

## Diagnose

- 404: the two settings are missing on the revision.
- 401: wrong audience, wrong service account, or the scheduler sends an OAuth token instead of OIDC.
- Stuck rows: `SELECT "Type", "Attempts", "NextAttemptAt", "LastError" FROM <schema>."OutboxMessages" WHERE "ProcessedAt" IS NULL ORDER BY "CreatedAt";`

Rollback: pause the scheduler job. Rows stay; the immediate try still runs on each request.
