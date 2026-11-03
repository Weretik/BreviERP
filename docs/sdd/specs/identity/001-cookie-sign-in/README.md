# Identity 001: Cookie sign-in like Kedr

**Status:** implemented 2026-11-03; not deployed. **Source plan:** [auth like Kedr](../../../../ideas/auth-like-kedr.md). **Reference:** KedrStore `docs/sdd/specs/identity/001-authentication-rollout/` (PRs KedrStore #138, #140).

## Goal

Admin signs in with email and password and keeps the session in an HttpOnly cookie on `api.brevi.com.ua`; the API is closed to everyone else. The temporary anonymous mode ([platform 002](../../platform/002-temporary-anonymous-api/README.md)) and the token endpoints are removed.

## Decisions

- D-1. Cookie only. Brevi has no mobile app, so there are no access or refresh tokens and no bearer scheme. A mobile client later adds them like KedrStore.
- D-2. Manager may do everything in Admin except «Користувачі» (owner decision R-2 in the plan follows Kedr). Role `User` stays for `CatalogRead` but no account has it.
- D-2a (owner, 2026-11-03). Staff roles Director («Директор»), Admin, Accountant («Бухгалтер»), Manager. One director; the director also holds Admin; only the director changes the director account and may change their own role. Accountant reads the catalogue only until its scope is agreed. Staff accounts can be deleted.
- D-3. First administrator `vitaliythupin@gmail.com` (owner, 2026-11-03), password from Secret Manager `admin-default-password`, created by the seed job even if other users exist. An existing password is never overwritten.
- D-4. API address through a Cloudflare Worker (`deploy/cloudflare/api-proxy`) with a shared secret for the client IP.
- D-5. Public operations: cookie login, Telegram webhook, `GET /api/reference/product-categories/store/{language}`.

## Requirements

| ID | Requirement |
| --- | --- |
| R-01 | `POST /api/auth/cookie/login` sets `__Host-brevi.session` (HttpOnly, Secure, Lax, sliding 7 days) and `brevi.csrf` (`Domain=brevi.com.ua` in Production); `401` for wrong credentials, lock or block. |
| R-02 | A request with the session cookie that is not GET/HEAD/OPTIONS needs `X-CSRF-Token` equal to `brevi.csrf`, else `400`. |
| R-03 | `POST /api/auth/cookie/logout` ends this session; `GET /api/auth/session/me` returns id, email, full name, roles. |
| R-04 | 5 wrong passwords lock any account for 15 minutes; passwords are at least 8 characters. |
| R-05 | Data Protection keys persist in `identity."DataProtectionKeys"`; the refresh-session table is dropped. |
| R-06 | `/api/admin/users` (Admin only): list, create Director/Admin/Accountant/Manager, change role, block/unblock, set password, delete; one director, only the director changes the director; no self-block or self-delete; only the director changes their own role. |
| R-07 | Every endpoint except D-5 requires a session, and every admin endpoint names a role or policy. |
| R-08 | Client IP from `X-Brevi-Client-IP` is trusted only with the matching `X-Brevi-Proxy-Secret`. |

## Delivery and evidence

| Stage | Scope | Evidence |
| --- | --- | --- |
| 1 | Hardening: Data Protection in DB, lockout, password length, seeder, trusted proxy | `TrustedProxyClientIpMiddlewareTests` (4); migration `PersistDataProtectionKeysAndDropRefreshSessions` |
| 2 | Cookie sign-in, CSRF, users API, token endpoints removed | `IntegrationTests.Identity` (9) on PostgreSQL 16 |
| 3 | Close API, remove anonymous mode | `ApiAccessTests` (5); `ProductsApiTests` (9, now signed in) |

Commands: `dotnet test tests/HostApiTests/HostApiTests.csproj`; `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter "FullyQualifiedName~IntegrationTests.Identity|FullyQualifiedName~ProductsApiTests"` with `BREVIERP_CATALOG_TEST_CONNECTION`.

## Rollout

Backend and brevi-web ship together: after this deploy Admin without the login page cannot work. Order: secrets → Worker → `api.brevi.com.ua/health` → merge both PRs → sync fork → first sign-in. Rollback: revert both PRs; the migration's Down recreates the empty refresh-session table.
