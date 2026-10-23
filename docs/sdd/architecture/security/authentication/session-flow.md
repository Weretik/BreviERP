# Authentication

## Current mechanism

BreviERP uses ASP.NET Core Identity with the Bearer token scheme (`IdentityConstants.BearerScheme`). Identity data and refresh session records are stored through `AppIdentityDbContext` in PostgreSQL. `Host.Api` enables authentication middleware before authorization.

```text
Client
  | POST /api/auth/session/login { email, password }
  ▼
AuthSessionController (AllowAnonymous, AuthLogin rate limit)
  ▼
SessionLoginCommand → Mediator behaviors → IdentitySessionService
  ▼
UserManager + SignInManager → AppUserClaimsPrincipalFactory
  ▼
access token in JSON response + refresh token in an HttpOnly cookie
```

`AppUserClaimsPrincipalFactory` adds the user ID and name, email, security stamp, user claims, and roles to the authenticated principal. Roles are therefore available to authorization policies.

## Session endpoints

| Endpoint | Access | Input and result |
| --- | --- | --- |
| `POST /api/auth/session/login` | `AllowAnonymous`, `AuthLogin` rate limit | Email/password. Returns the token type, access token, and expiration; sets refresh and CSRF cookies. |
| `POST /api/auth/session/refresh` | `AllowAnonymous`, `AuthRefresh` rate limit | Requires the refresh-token cookie and a configured CSRF header matching the CSRF cookie. Rotates the refresh session and returns a new access token. |
| `POST /api/auth/session/logout` | authenticated user | Revokes active refresh sessions, updates the security stamp, then clears the refresh and CSRF cookies. |
| `GET /api/auth/session/me` | authenticated user | Returns the current user's ID, email, and roles. |

The refresh token is never returned in a JSON response. Its cookie is `HttpOnly`; the browser can read the CSRF cookie so the frontend can send its value in the configured header for refresh requests.

## Token and session rules

- Access and refresh token lifetimes come from `IdentitySessionSecurityOptions` when `AddBearerToken` is registered.
- Each refresh token is stored only as a SHA-256 hash in `RefreshSessions`, together with timestamps and limited client context.
- Refresh sessions have an absolute lifetime and an inactivity lifetime.
- A successful refresh rotates the session: the previous session is revoked and linked to its replacement.
- Reuse of a revoked or replaced refresh token revokes all active refresh sessions for that user.
- Logout also changes the user's security stamp, invalidating refresh tokens tied to the old stamp.

## Current Identity settings

- A unique email is required.
- Password length is at least 5 characters.
- Digits, lowercase and uppercase letters, and non-alphanumeric characters are not currently required.

These are current runtime settings, not a recommendation for a public registration feature. Any public registration design must explicitly review password policy, email verification, abuse protection, and recovery flows before implementation.

## Frontend usage

1. Call `login`; store the returned access token according to the frontend security design.
2. Send it as `Authorization: Bearer <access-token>` for protected API requests.
3. During refresh, allow the browser to send cookies and add the configured CSRF header value from the CSRF cookie.
4. During logout, call `logout` while the access token is still valid, then discard the access token on the client.

Do not put the refresh token in local storage, application logs, or the frontend request body.
