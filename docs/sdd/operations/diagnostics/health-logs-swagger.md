# Diagnostics: health, logs, and Swagger

## Health check

Both endpoints allow anonymous access and are not traced.

| Endpoint | Checks | Answers | Use it for |
| --- | --- | --- | --- |
| `GET /health` | Nothing but the process (liveness) | `200 Healthy` while the host answers | Is the host running and reachable |
| `GET /health/ready` | PostgreSQL: opens a connection with `ConnectionStrings:Default` and runs `SELECT 1` (`DatabaseHealthCheck`) | `200 Healthy`, or `503 Unhealthy` when the database is unreachable | Uptime alerts (`https://api.brevi.com.ua/health/ready`) and diagnosing "the site is up but nothing loads" |

The reason of a failed check goes only to the log, never to the response. Liveness deliberately ignores the database, so a database outage never makes Cloud Run restart healthy instances.

## Swagger and OpenAPI

Swagger UI is enabled only when `ASPNETCORE_ENVIRONMENT=Development`. With the HTTPS launch profile, open:

```text
https://localhost:7142/swagger
```

Swagger UI reads the built-in document `/openapi/v1.json`, the same one the build writes to `docs/sdd/contracts/openapi.json`. Sign in with `POST /api/auth/cookie/login` from Swagger on the same host; the browser keeps the session cookie. For `POST`, `PUT` and `DELETE` also add header `X-CSRF-Token` with the `brevi.csrf` cookie value, otherwise the API answers `400 Invalid CSRF token.`

For a reproducible feature test, add a specific task to the verification phase of the [feature specification](../../specs/README.md). It should record the route, prerequisites, input, expected status, and failure cases.

## Logs and startup failures

Serilog is configured by the host and writes to the console: plain text locally, Cloud Logging JSON in production (one object per line with `severity` and the trace id); Seq is also supported when configured. HTTP request logging is enabled in the pipeline. If `Host.Api` fails to start, first check its console output, including configuration and database connection errors. A separate `Host.Seed` runs migrations and seeding; investigate their failures in that host's output.

Do not log or share tokens, passwords, full connection strings, cookie values, or raw confidential request data. For a protected route problem, distinguish these cases before changing code:

| Observation | First check |
| --- | --- |
| `401 Unauthorized` | The request carries the `__Host-brevi.session` cookie (credentials mode `include`, API on `api.brevi.com.ua`). |
| `403 Forbidden` | The authenticated user has the role/policy required by the endpoint. |
| `400 Invalid CSRF token.` | The changing request sends `X-CSRF-Token` equal to the `brevi.csrf` cookie. |
| Browser request blocked | Allowed CORS origin and HTTPS/cookie attributes. |
| `Host.Api` crashes before serving requests | Database connection and required settings; if `Host.Seed` ran separately beforehand, check its result. |

## Traces in Google Cloud

In production every API request is a trace in Cloud Trace, service `brevierp-api` ([PLT-004](../../specs/platform/004-request-tracing/README.md), [ADR-0017](../../architecture/decisions/0017-opentelemetry-traces-to-cloud-trace.md)). A trace shows the endpoint, each SQL command and outgoing call (Telegram, Gmail, KeepinCRM, OpenAI, CDN) with its duration, and the exception if one was thrown.

**From a log line to the trace.** Cloud Console → Logging → Logs Explorer, query `resource.type="cloud_run_revision" resource.labels.service_name="brevierp-api" severity>=ERROR`. Open a line and choose «View trace details» next to its trace id.

**From a trace to its logs.** Cloud Console → Trace → Trace Explorer, filter service `brevierp-api` (or status error, or latency above a value), open a trace and choose «View logs».

**No spans from the API.** When the trace has only the Cloud Run spans (`AppServer`, `HTTP load balancer`) and nothing from the service, the export fails. The SDK's own warnings and errors are in the API log under logger `BuildingBlocks.Observability.OpenTelemetryErrorLog` (for example 403 from the Telemetry API); query `jsonPayload.logger="BuildingBlocks.Observability.OpenTelemetryErrorLog"`.

**From the command line** (read-only):

```bash
gcloud logging read 'resource.type="cloud_run_revision" AND resource.labels.service_name="brevierp-api" AND severity>=ERROR' \
  --project brevistoreapi --freshness 1d --limit 20 --format json
# every entry has trace: projects/brevistoreapi/traces/<id>; all lines of that request:
gcloud logging read 'trace="projects/brevistoreapi/traces/<id>"' --project brevistoreapi
```

**One-time setup in the project** (owner, project `brevistoreapi`): enable the APIs `telemetry.googleapis.com` and `cloudtrace.googleapis.com`; give the Cloud Run runtime account of `brevierp-api` the role Cloud Trace Agent (`roles/cloudtrace.agent`). Until then the API works and only the traces are missing; the JSON logs work without this step.

**Settings:** `Observability:GoogleCloudProjectId` turns export and JSON logs on (production only); `Observability:TraceSampleRatio` (0…1) keeps that share of traces if the free tier is ever exceeded; `Observability:ServiceName` names the service in Cloud Trace.
