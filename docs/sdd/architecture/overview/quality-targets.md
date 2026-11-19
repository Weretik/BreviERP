# Quality targets

Measurable goals for the Brevi production system. They tell when the system is "slow" or "down" before a user does, they are the thresholds the alerts use, and **every architecture review, audit and feature design starts from them**: a speed, reliability or scaling verdict without these numbers is not given.

Status of each row:

- **confirmed**: the owner confirmed the number on 2026-11-17; change it only together with its alert.
- **proposed**: added on 2026-11-19 to close the gaps of the first version; the number is a starting point to confirm with the owner after the first measurement.

The **Current** column holds the last measured value with its date. "Not measured" is an open gap, not a pass; the [measurement plan](#measurement-plan) says how to fill it.

## Scale the targets are set for

| What | Value | Source |
| --- | --- | --- |
| Users | A small staff team in the admin (managers, accountant, workshop), visitors of the public site «Строчка» (phone orders only, no online checkout) | Product description, [glossary](../../../product/glossary.md) |
| Catalogue | About 124 sewing models with 248 articles and 119 PPE items (import of November 2026) | Client spreadsheet import summary; confirm with the row count below |
| API requests | About 10 400 a day; peak 1 910 a minute (2026-11-12 19:05 UTC) and 17 180 an hour | Cloud Run request logs, 2026-10-30 → 2026-11-19 (measured 2026-11-19) |
| КП per day, site requests per day | Not measured | Measurement plan, step 4 |
| Hosting | API `brevierp-api` on Cloud Run in `europe-central2`, 0 to 20 instances (no minimum instance, so the first request after a quiet period waits for a cold start), 1 vCPU and 512 MiB each, 80 requests per instance, CPU only during requests; PostgreSQL managed on adm.tools (outside Google Cloud, so every query crosses the network between the two); public site and the API front door `api.brevi.com.ua` on Cloudflare Workers | [Deploy workflow](../../../../.github/workflows/deploy-cloudrun.yml), [backups](../../operations/data/backups-and-restore.md) |
| Design horizon (proposed) | ×10 the current catalogue, КП and workshop volume without a change of architecture | Checked in reviews against the measured volume |

## Speed

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Reads, P95 | 95% of `GET` requests answer within 1 second | confirmed | **Met:** 89 ms (P50 32 ms, 196 239 requests, 2026-10-30 → 2026-11-19) | Cloud Run request latency of `brevierp-api` (service and traces) |
| Reads, P99 | 99% of `GET` requests answer within 3 seconds | proposed | **Met:** 627 ms (same window) | Same |
| Writes, P95 | 95% of other requests answer within 2 seconds | confirmed | **Met:** 1 566 ms (2 547 requests, same window) | Same |
| Writes, P99 | 99% of other requests answer within 5 seconds, without the accepted long operations below | proposed | 13 040 ms with them; most of it comes from the accepted long operations. Recompute without them at the next monthly check | Same |
| Long operations | A КП PDF, a bank statement preview or an import preview answers within 10 seconds. **Accepted exception (owner, 2026-11-19):** rare or one-off staff operations (sewing price recalculation, old-site photo import, AI recognition of a purchase file) may take up to 2 minutes, well under the 5-minute request timeout, and are left out of the P99 below | proposed | **Met** for current endpoints. The 54 s P95 seen for `GET /api/v1/products/overview/prices` came from calls before 2026-11-03, when [CAT-013](../../specs/catalog/013-product-list-page/README.md) removed that endpoint (the list is now one paged request). Accepted exceptions measured: recalculation 70 s, photo import 59 s, AI recognition 32 s | Latency of those endpoints in the same logs |
| Public site pages | At the 75th percentile of mobile visits: LCP ≤ 2.5 s, INP ≤ 200 ms, CLS ≤ 0.1 (Google's "good" Core Web Vitals) | proposed | Not measured | Search Console «Core Web Vitals»; PageSpeed Insights for the home, a category and a product page while field data is too small |
| Peak capacity | The API holds three times the measured peak requests per minute with reads still under their P95 target | proposed | Measured peak 1 910 requests a minute (2026-11-12 19:05 UTC), 17 180 an hour; about 10 400 requests a day. The ×3 load was never tested | Measurement plan, step 2 |

A cold start of a new Cloud Run instance falls into P99. Measured on 2026-11-19: the first request after more than 15 quiet minutes took 4.3 s at the median and 6.6 s at P95 (224 cases in 19 days); the CRM lead pages are slow only at this tail. If P99 misses because of cold starts, the decision (a minimum instance, which costs money every month) is made with the cost row below, not by changing the target.

## Availability and errors

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Availability of the API | 99.5% of each month (about 3.5 hours of downtime a month) | confirmed | Not measured: **no uptime check exists** | Uptime check on the API's `/health/ready`; share of minutes without 5xx bursts in the Cloud Run request logs |
| Server errors | Under 1% of requests answer 5xx in any 15 minutes | confirmed | **Missed once:** 0.01% of all requests answered 5xx, but on 2026-11-08 23:00 one 15-minute window had 14% (3 of 21). **No alert exists** | Cloud Run request logs, alert «API 5xx» |
| Background tasks | A scheduled task that fails or does not run for twice its period is noticed within 1 hour | confirmed | **No alert exists** | «Фонові задачі» page and job error logs, alert «Job failed» |

## Data freshness and delivery

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Site requests and messages | A site request or a «Надіслати в цех» message is delivered within 15 minutes | confirmed | Not measured | Outbox attempts; a message that runs out of attempts is an incident and sends the Telegram alert |
| Data from KeepinCRM and the client's sheets | Clients, suppliers, leads and purchase invoices are never older than one working day (weekday 09:00 runs) | proposed | Tasks enabled: to check | Last success of each task on «Фонові задачі» |
| PPE supplier prices | Never older than the supplier's day in «Імпорт цін» plus one working day | proposed | Task enabled: to check | Same |
| Telegram reminders | Sent on the working day they are due, by 09:30 | proposed | Task enabled: to check | Same, and the group in Telegram |

These tasks are off by default in the code; a target counts only once its task is enabled on «Фонові задачі».

## Data safety

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Data loss (RPO) | At most 24 hours of data, the age of the latest hosting backup | confirmed | Met by design: a backup every day, kept 2 months (owner, 2026-11-19); the age of the latest backup is not alerted | [Backups and restore](../../operations/data/backups-and-restore.md) |
| Recovery time (RTO) | Service back within 4 hours after the database is lost | confirmed | Never timed: no restore drill yet | Restore procedure in the same document |

## Cost

| Target | Value | Status | Current | Measured by |
| --- | --- | --- | --- | --- |
| Monthly running cost | A budget the owner sets for Google Cloud, Cloudflare and adm.tools together; a Google Cloud budget alert at 50%, 90% and 100% | proposed, number to set | Not measured | Google Cloud Billing reports and the other providers' invoices |
| AI recognition of purchase documents | A monthly limit on the OpenAI account used by «Розпізнавання оригіналів закупки», set by the owner, with the provider's usage limit at that value | proposed, number to set | Not measured | OpenAI usage page |

## Gaps in measurement

Baseline measured on 2026-11-19 from 19 days of Cloud Run request logs (the newest 200 000 rows, 2026-10-30 → 2026-11-19). Each gap is open until the target has a current value and an alert that is known to exist.

1. **No alerts and no uptime check at all.** «API 5xx», «Job failed», the P95 alert and the uptime check are named here and in [dependency failures](../../operations/diagnostics/dependency-failures.md) but do not exist in Cloud Monitoring. Only the outbox give-up message in Telegram is in the code.
2. **Long operations are accepted:** sewing price recalculation, old-site photo import and AI recognition are rare staff actions the owner accepted on 2026-11-19 as allowed to take up to 2 minutes. The slow products price overview in the logs is the endpoint CAT-013 removed on 2026-11-03.
3. **1 533 requests answered 429** (too many requests) in 19 days. Which endpoints and whether the limits or the Cloudflare rule are too tight for the staff is not yet known.
4. **A 5xx burst went unnoticed:** 14% of requests failed in one 15-minute window on 2026-11-08 23:00 UTC; its cause is not recorded.
5. **No availability figure** without an uptime check, and no dashboard shows P95, P99 and the error share against these targets.
6. **Restore never tried.** Backups run daily and are kept 2 months (owner, 2026-11-19), but a restore was never timed, so RTO is an assumption.
7. **No budget alert,** no monthly cost figure, and no spending limit recorded for the AI recognition.
8. **Core Web Vitals** of the public site were never checked; freshness tasks were not checked as enabled.
9. **`claude-readonly` cannot read Cloud Run settings** (`run.services.get` denied); the settings above come from the deploy audit log. Give it `roles/run.viewer` to read them directly.

## Measurement plan

Read-only steps; none changes the system. The owner runs the console steps; an agent can run the `gcloud` steps through Remote Control on the owner's computer with impersonation (`--impersonate-service-account=claude-readonly@<project>.iam.gserviceaccount.com`).

1. **Read-only access.** `claude-readonly` has `roles/monitoring.viewer` since 2026-11-19; add `roles/run.viewer` to read the Cloud Run settings too. With these, latency, request count and alert policies can be read without logs: `gcloud alpha monitoring policies list`, and the metrics `run.googleapis.com/request_latencies` and `run.googleapis.com/request_count` of service `brevierp-api`.
2. **Latency and peak from request logs** (Logs Viewer is enough): read 30 days of `resource.type="cloud_run_revision" AND resource.labels.service_name="brevierp-api"` with `--format="csv(timestamp,httpRequest.requestMethod,httpRequest.requestUrl,httpRequest.latency,httpRequest.status)"`, then compute P95 and P99 for `GET` and for other methods, the same for the long operations, the 5xx share per 15 minutes, and the busiest minute. Write the numbers and the date into **Current**. A busy month has more than 200 000 log rows: export one day at a time, or the window is cut short.
3. **Alerts.** In Cloud Monitoring → Alerting, list the policies. Create the missing ones from the thresholds above: uptime check on `/health/ready`, 5xx share over 1% for 15 minutes, P95 of `GET` over 1 s for 15 minutes, failed job. Name them as this document does and send them to the owner.
4. **Data volume.** In the adm.tools database console, read-only: `SELECT pg_size_pretty(pg_database_size(current_database()));`, `SELECT schemaname, relname, n_live_tup FROM pg_stat_user_tables ORDER BY n_live_tup DESC LIMIT 15;`, and the number of rows in `crm."Quotes"` and `crm."SiteRequests"` created per day over the last 90 days.
5. **Backups.** Frequency and retention are recorded (daily, 2 months). Once a quarter restore the latest backup into a new database and time it: that time is the current RTO.
6. **Site.** Search Console → «Core Web Vitals» for mobile and desktop.
7. **Cost.** Google Cloud Billing → Reports for the last three months, the other providers' invoices and the OpenAI usage page; the owner sets the budgets and creates the budget alert.

## How they are used

- Every architecture review, code audit and new-feature design starts by reading this file. Missing targets or missing current values are the review's first finding; a feature on a hot path (КП, catalogue and prices, workshop, site pages, imports) states its expected load and checks it does not move P95 or P99 past the target.
- Alerts in Cloud Monitoring use these thresholds and notify the owner.
- Once a month compare the last 30 days with the table and update **Current**; a target missed twice in a row becomes a task, not a new number.
- A proposed row becomes confirmed when the owner accepts its number; then it gets its alert.
