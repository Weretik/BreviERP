# Diagnostics: health, logs, and Swagger

## Health check

`GET /health` is mapped by `Host.Api` and explicitly allows anonymous access. Use it first to determine whether the host is running and reachable.

## Swagger and OpenAPI

Swagger UI is enabled only when `ASPNETCORE_ENVIRONMENT=Development`. With the HTTPS launch profile, open:

```text
https://localhost:7142/swagger
```

Sign in with `POST /api/auth/cookie/login` from Swagger on the same host; the browser keeps the session cookie. For `POST`, `PUT` and `DELETE` also add header `X-CSRF-Token` with the `brevi.csrf` cookie value, otherwise the API answers `400 Invalid CSRF token.`

For a reproducible feature test, add a specific task to the verification phase of the [feature specification](../../specs/README.md). It should record the route, prerequisites, input, expected status, and failure cases.

## Logs and startup failures

Serilog is configured by the host and writes to the console; Seq is also supported when configured. HTTP request logging is enabled in the pipeline. If `Host.Api` fails to start, first check its console output, including configuration and database connection errors. A separate `Host.Seed` runs migrations and seeding; investigate their failures in that host's output.

Do not log or share tokens, passwords, full connection strings, cookie values, or raw confidential request data. For a protected route problem, distinguish these cases before changing code:

| Observation | First check |
| --- | --- |
| `401 Unauthorized` | The request carries the `__Host-brevi.session` cookie (credentials mode `include`, API on `api.brevi.com.ua`). |
| `403 Forbidden` | The authenticated user has the role/policy required by the endpoint. |
| `400 Invalid CSRF token.` | The changing request sends `X-CSRF-Token` equal to the `brevi.csrf` cookie. |
| Browser request blocked | Allowed CORS origin and HTTPS/cookie attributes. |
| `Host.Api` crashes before serving requests | Database connection and required settings; if `Host.Seed` ran separately beforehand, check its result. |
