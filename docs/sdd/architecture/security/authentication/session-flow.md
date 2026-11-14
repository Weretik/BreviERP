# Authentication

## Current mechanism

BreviERP has one sign-in scheme: the ASP.NET Core Identity session cookie (`IdentityConstants.ApplicationScheme`). Admin (React) is the only client. There are no access or refresh tokens and no bearer scheme; a mobile client would add them later the way KedrStore does.

```text
Admin (office.brevi.com.ua)
  | POST https://api.brevi.com.ua/api/auth/cookie/login { email, password }
  ▼
Cloudflare Worker (tools/cloudflare/api-proxy) → Cloud Run Host.Api
  ▼
LoginCookieSessionEndpoint (AllowAnonymous, AuthLogin rate limit)
  ▼
CookieLoginCommand → Mediator behaviors (validation) → CookieSessionService
  ▼
SignInManager.PasswordSignInAsync (lockoutOnFailure) → AppUserClaimsPrincipalFactory
  ▼
Set-Cookie: __Host-brevi.session (HttpOnly) + brevi.csrf (readable, Domain=brevi.com.ua)
```

The API must be on the same site as Admin (`brevi.com.ua`), otherwise the browser treats the session cookie as third-party and Admin cannot read the CSRF cookie. That is why the API is served on `api.brevi.com.ua` through the [Cloudflare Worker](../../../operations/deployment/cloudflare-api-proxy.md).

## Endpoints

| Endpoint | Access | Result |
| --- | --- | --- |
| `POST /api/auth/cookie/login` | `AllowAnonymous`, `AuthLogin` rate limit (10 per minute per client IP) | Signs in; returns the current user (`SessionMeResponse`) and sets both cookies. Wrong credentials, a locked or a blocked account answer `401`. |
| `POST /api/auth/cookie/logout` | signed in, CSRF header | Ends this browser's session and expires both cookies. Other devices stay signed in. |
| `GET /api/auth/session/me` | signed in | User ID, email, full name, roles. Reissues a missing CSRF cookie. |

The old token endpoints `POST /api/auth/session/login`, `/refresh` and `/logout`, the `kedr.rt` refresh cookie and the refresh-session table are removed.

## Cookies

| Cookie | Set by | Attributes |
| --- | --- | --- |
| `__Host-brevi.session` | Identity | HttpOnly, Secure, SameSite=Lax, Path=/, no Domain; sliding 7 days (`Identity:CookieSession`). |
| `brevi.csrf` | `SessionCookieWriter` | Readable by script, Secure, SameSite=Lax, Path=/, `Domain=brevi.com.ua` in Production (`Identity:SessionCookies:CsrfCookieDomain`), 14 days. |

`CookieCsrfMiddleware` (Host.Api) checks every request that carries the session cookie and is not `GET`, `HEAD` or `OPTIONS`: header `X-CSRF-Token` must equal the `brevi.csrf` cookie, otherwise `400 Invalid CSRF token.` Login is exempt. On a `GET` with a session and no CSRF cookie, it issues a new one.

Unauthenticated requests get `401` and requests without the needed role get `403`; the cookie handler never redirects.

## Session rules

- Data Protection keys, which sign the cookie, are stored in the `identity."DataProtectionKeys"` table (`SetApplicationName("BreviERP")`). Every Cloud Run instance shares them, and a deploy does not sign anybody out.
- The security stamp is checked every 5 minutes. Changing a role, blocking or setting a new password changes the stamp, so the user's sessions end within 5 minutes.
- Lockout applies to every account, including the administrator: 5 wrong passwords lock it for 15 minutes.
- Passwords are at least 8 characters.
- The client IP for the rate limit comes from the Worker's `X-Brevi-Client-IP` only when `X-Brevi-Proxy-Secret` matches `TrustedProxy:SharedSecret` (`TrustedProxyClientIpMiddleware`).

## First administrator

There is no open registration. `IdentitySeeder` (seed job) creates `Identity:AdminUser:Email` with the password from the `ADMIN_DEFAULT_PASSWORD` secret when that email has no account; without the secret it creates nothing. It never changes an existing password. Further administrators and managers are created on the Admin «Користувачі» page ([staff accounts](../../../features/identity/staff-accounts.md)).

## Frontend usage

1. Send every request with credentials (`withCredentials: true`); never store a token in JavaScript.
2. On start, call `GET /api/auth/session/me`: `200` means signed in, `401` means show the login page.
3. Add `X-CSRF-Token` with the `brevi.csrf` cookie value to every `POST`, `PUT`, `PATCH` and `DELETE`.
4. On `401` go to the login page; on `403` show «Немає доступу».
