# User Session

**Static code review:** 2026-10-16. **Module:** identity.

Login, refresh, logout, and `me`.

## Structure and relationships

[AuthSessionController](../../../../src/Modules/Identity/Identity.Api/Identity.Api/Controllers/AuthSessionController.cs) calls the [login handler](../../../../src/Modules/Identity/Identity.Application/Identity.Application/Features/Auth/SessionLogin/SessionLoginCommandHandler.cs) and neighboring handlers. [IdentitySessionService](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Services/IdentitySessionService.cs) and [Identity DbContext](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/DataBase/AppIdentityDbContext.cs) own session and user state; see the [flow](../../architecture/security/authentication/session-flow.md) for cookie and CSRF rules.

## Code structure

The following are verified entry points and key feature components; the tree does not list every supporting file.

```text
src/
└── Modules/Identity/
    ├── Identity.Api/Identity.Api/Controllers/AuthSessionController.cs  # HTTP entry point
    ├── Identity.Application/Identity.Application/Features/Auth/
    │   ├── SessionLogin/SessionLoginCommandHandler.cs  # use-case handler
    │   ├── SessionLogout/SessionLogoutCommandHandler.cs  # use-case handler
    │   ├── SessionMe/SessionMeQueryHandler.cs  # use-case handler
    │   └── SessionRefresh/SessionRefreshCommandHandler.cs  # use-case handler
    └── Identity.Infrastructure/Identity.Infrastructure/
        ├── DataBase/AppIdentityDbContext.cs  # data context
        └── Services/IdentitySessionService.cs  # infrastructure implementation
```

## Structure diagram

```mermaid
flowchart LR
    A["AuthSessionController"] --> B["SessionLoginCommandHandler and other handlers"]
    B --> C["IdentitySessionService"]
    C --> D["AppIdentityDbContext"]
    A --> E["Cookie and CSRF"]
```

## Related sources

[OpenAPI](../../contracts/identity/session.openapi.yaml).
