# Identity, Authentication, and Authorization

This architecture area covers user accounts, authentication, authorization, and access-control planning. It intentionally sits outside `platform`: this section explains how the application uses the technologies described under `platform`.

```text
security/
├── authentication/  login, bearer tokens, refresh, logout, and CSRF
├── authorization/   fallback protection, roles, and named policies
├── users/           initial users and registration design
└── planning/        SDD checklist for access-controlled features
```

## Read by task

- Session, token, or refresh issue: [authentication](authentication/README.md)
- Protecting an endpoint or adding a policy or role: [authorization](authorization/README.md)
- User creation, registration, or invitations: [users](users/README.md)
- Planning a permission-controlled feature: [planning checklist](planning/access-control-checklist.md)

## Responsibility split

```text
Identity.Api             HTTP sessions and future Identity endpoints
        │
Identity.Application     commands/queries, validators, and service contracts
        │
Identity.Infrastructure  ASP.NET Core Identity, tokens, sessions, AppIdentityDbContext
        │
Identity.Domain          role names

Host.Api                 authentication middleware and authorization policies
```

Identity owns accounts and credentials. A business module may protect an endpoint with a policy, but must not create users, verify passwords, or access `UserManager<AppUser>` directly.
