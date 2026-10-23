# Users, Bootstrap, and Registration

## What Exists Today

The current API has no public self-registration endpoint. The session API supports only login, refresh, logout, and `me`.

Current user creation paths:

| Path | Behavior |
| --- | --- |
| Startup bootstrap | `RoleSeeder` creates the seed roles. `IdentitySeeder` creates the configured administrator only when one does not already exist and `ADMIN_DEFAULT_PASSWORD` is set. |

The default administrator is configured through `AdminUserConfig`; its password is read from `ADMIN_DEFAULT_PASSWORD`. Never commit this value to configuration or documentation.

## Designing New Registration or User Management Features

Treat this as Identity module functionality. It is not an implementation detail of Catalog, Reference, or an API controller.

```text
Identity.Api endpoint
  → Identity.Application command + validator
  → application service contract
  → Identity.Infrastructure implementation
  → UserManager<AppUser> / RoleManager<AppRole> / AppIdentityDbContext
```

Minimum decisions for the feature specification:

- Who can create users: anonymous self-registration, Admin only, import only, or another policy.
- Identity fields and validation: email uniqueness, normalized input, full name, and password rules.
- The initial role and whether the caller can choose it. A public request must never grant Admin or Manager.
- Account activation: immediate sign-in, email verification, invitation, or administrator approval.
- Password setup and reset, and the recovery flow.
- Abuse protection: rate limits, audit logs, and CAPTCHA/email verification requirements.
- What happens to active sessions when roles, passwords, or account status change.
- API response shape: never return a password, refresh token, password reset token, or internal Identity errors.

## Implementation Boundaries

- Place the HTTP contract and controller in `Identity.Api`.
- Place commands/queries, FluentValidation validators, Ardalis.Result contracts, and application-facing interfaces in `Identity.Application`.
- Place ASP.NET Core Identity calls, `AppUser` persistence, role assignment, and token provider calls in `Identity.Infrastructure`.
- Keep only stable role constants in `Identity.Domain`; do not put HTTP DTOs, `UserManager`, or EF dependencies there.
- Register the new service through the existing Identity module registration order; Host.Api composes it through `AddHostServices`.

Use `UserManager` errors to produce a safe, consistent Application result. Do not expose raw Identity diagnostics to anonymous callers.
