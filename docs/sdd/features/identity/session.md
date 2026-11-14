# User Session

**Module:** identity.

Browser sign-in for Admin with the Identity session cookie: login, logout and `me`. The [flow](../../architecture/security/authentication/session-flow.md) describes cookies, CSRF, lockout and the first administrator.

## Structure and relationships

[CookieAuthController](../../../../src/Modules/Identity/Identity.Api/Identity.Api/Controllers/CookieAuthController.cs) and [AuthSessionController](../../../../src/Modules/Identity/Identity.Api/Identity.Api/Controllers/AuthSessionController.cs) send commands to handlers in `Features/Auth`, which call [CookieSessionService](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Integrations/AspNetIdentity/Sessions/CookieSessionService.cs) (`SignInManager`). [SessionCookieWriter](../../../../src/Modules/Identity/Identity.Api/Identity.Api/Cookies/SessionCookieWriter.cs) writes the CSRF cookie, and Host.Api's [CookieCsrfMiddleware](../../../../src/Bootstrapper/Host.Api/Security/Csrf/CookieCsrfMiddleware.cs) checks it. [IdentityAuthenticationExtensions](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/DependencyInjection/IdentityAuthenticationExtensions.cs) configures the cookie; Data Protection keys live in [AppIdentityDbContext](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Persistence/AppIdentityDbContext.cs).

## Code structure

```text
src/
├── Bootstrapper/Host.Api/Security/
│   ├── Csrf/CookieCsrfMiddleware.cs                 # CSRF header check for cookie sessions
│   └── TrustedProxy/TrustedProxyClientIpMiddleware.cs # client IP from the Cloudflare Worker
└── Modules/Identity/
    ├── Identity.Api/Identity.Api/
    │   ├── Controllers/CookieAuthController.cs      # login, logout
    │   ├── Controllers/AuthSessionController.cs     # me
    │   └── Cookies/SessionCookieWriter.cs           # CSRF cookie
    ├── Identity.Application/Identity.Application/Features/Auth/
    │   ├── Commands/CookieLogin/                    # command, handler, validator
    │   ├── Commands/CookieLogout/
    │   └── Queries/SessionMe/
    └── Identity.Infrastructure/Identity.Infrastructure/
        ├── DependencyInjection/IdentityAuthenticationExtensions.cs # cookie scheme, 401/403
        ├── DependencyInjection/IdentityDbContextExtensions.cs      # lockout, password, Data Protection
        ├── Integrations/AspNetIdentity/Sessions/CookieSessionService.cs
        └── Persistence/Seeding/IdentitySeeder.cs                # first administrator
```

## Tests

- `tests/IntegrationTests/Identity/CookieAuthApiTests.cs`: login sets both cookies, `me`, wrong password, `401` without session, CSRF check on logout, old token endpoints gone. Real Identity on PostgreSQL (`BREVIERP_CATALOG_TEST_CONNECTION`).
- `tests/HostApiTests/TrustedProxyClientIpMiddlewareTests.cs`, `tests/HostApiTests/ApiAccessTests.cs`.
- `tests/UnitTests/SessionOpenApiContractTests.cs`: contract shape.

Focused runs: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~IntegrationTests.Identity`, `dotnet test tests/HostApiTests/HostApiTests.csproj`.

**Data:** [identity schema](../../architecture/data-model/identity.md)

## Related sources

[OpenAPI](../../contracts/identity/session.openapi.yaml), [Cloudflare API proxy](../../operations/deployment/cloudflare-api-proxy.md).
