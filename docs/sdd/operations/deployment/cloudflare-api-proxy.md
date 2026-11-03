# Cloudflare API proxy

The public API address is `api.brevi.com.ua`. A Cloudflare Worker serves that host and forwards every request to the Cloud Run service `brevierp-api`. Cloud Run domain mapping is not available in `europe-central2`, and a Google load balancer costs a fixed monthly fee, so the Worker is the entry point (the same as KedrStore's `api.classkedr.com.ua`).

```text
Admin (office.brevi.com.ua)
  | https://api.brevi.com.ua/...
  v
Cloudflare Worker (deploy/cloudflare/api-proxy)
  | adds X-Brevi-Client-IP + X-Brevi-Proxy-Secret
  v
Cloud Run Host.Api (brevierp-api-443622778364.europe-central2.run.app)
  | TrustedProxyClientIpMiddleware restores the client IP
  v
rate limiting, sign-in, audit logs
```

## Why the API needs this host

Admin and the API must be on the same site (`brevi.com.ua`):

- The session cookie `__Host-brevi.session` is first-party, so browsers that block third-party cookies keep it. It is `SameSite=Lax`, which a request from `office.brevi.com.ua` to `api.brevi.com.ua` still carries.
- The CSRF cookie is set with `Domain=brevi.com.ua` (`Identity:SessionCookies:CsrfCookieDomain` in `appsettings.Production.json`), so Admin can read it and send it in `X-CSRF-Token`. A cookie set by `*.run.app` is never readable by a `brevi.com.ua` page.

## Client IP behind the proxy

Cloud Run sees the Worker's egress IP, not the user's. The Worker sends the original `CF-Connecting-IP` in `X-Brevi-Client-IP`, together with a shared secret in `X-Brevi-Proxy-Secret`. `TrustedProxyClientIpMiddleware` replaces `RemoteIpAddress` only when the secret matches, and always removes both headers, so a direct request to the Cloud Run URL cannot spoof its IP.

| Setting | Where | Secret |
| --- | --- | --- |
| `TrustedProxy:SharedSecret` | Cloud Run, from Secret Manager `api-proxy-shared-secret` (deploy workflow) | Yes |
| `PROXY_SHARED_SECRET` | Worker secret, same value | Yes |
| `ORIGIN_URL` | Worker variable, the Cloud Run service URL | No |
| `TrustedProxy:ClientIpHeaderName`, `SecretHeaderName` | `appsettings.json`; must match `worker.js` | No |

With an empty `TrustedProxy:SharedSecret` the middleware only strips the headers.

## Setup order

Both secrets must exist before the deploy that contains the cookie sign-in, because the deploy workflow attaches them and fails when one is missing.

1. Create Secret Manager secrets `api-proxy-shared-secret` (random, for example `openssl rand -hex 32`) and `admin-default-password` (the first administrator's password, at least 8 characters), and grant the Cloud Run runtime service account `Secret Manager Secret Accessor` on both.
2. Create the Worker from `deploy/cloudflare/api-proxy` (dashboard or `npx wrangler deploy`), set `ORIGIN_URL` and the secret `PROXY_SHARED_SECRET`, and add the custom domain `api.brevi.com.ua`. The `brevi.com.ua` zone must be in the same Cloudflare account.
3. Check `https://api.brevi.com.ua/health` (`Healthy`).
4. Merge BreviERP and brevi-web together and sync the deploy fork. The seed job creates `vitaliythupin@gmail.com` (`Identity:AdminUser:Email`) with the `admin-default-password` value.
5. Sign in on `https://office.brevi.com.ua`, create the staff accounts on «Користувачі» and block old accounts that nobody uses.

## Limits

- The Workers free plan allows 100,000 requests per day.
- The Cloud Run URL stays publicly reachable. The proxy is not access control; every endpoint enforces its own authorization.
- Admin on `localhost` calling the deployed API is cross-site: the cookies do not work there. Use a local API for development.
- Scripts that call the API (for example the sheet import in [migrations and seeding](../data/migrations-and-seeding.md)) use `https://api.brevi.com.ua`, because the CSRF cookie belongs to `brevi.com.ua`.
