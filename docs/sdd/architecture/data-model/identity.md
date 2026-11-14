# `identity` — Identity Module Data

**Source:** [Identity snapshot](../../../../src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure/Persistence/Migrations/AppIdentityDbContextModelSnapshot.cs), verified 2026-10-21. [Back to database map](README.md) · [session feature](../../features/identity/session.md).

| Table | PK | Purpose |
| --- | --- | --- |
| `AspNetUsers` | `Id` | Users and credentials. |
| `AspNetRoles` | `Id` | Roles and access levels. |
| `AspNetUserRoles` | `UserId`, `RoleId` | Role assignments. |
| `AspNetUserClaims` | `Id` | User claims (`UserId` FK). |
| `AspNetRoleClaims` | `Id` | Role claims (`RoleId` FK). |
| `AspNetUserLogins` | `LoginProvider`, `ProviderKey` | External logins (`UserId` FK). |
| `AspNetUserTokens` | `UserId`, `LoginProvider`, `Name` | User tokens (`UserId` FK). |
| `DataProtectionKeys` | `Id` | ASP.NET Core Data Protection keys that sign the session cookie; shared by all API instances. |
| `role_access` | `role_id` | «Права доступу»: saved rights of a role (page → action flags, `jsonb`), `version` for stale-save checks, who and when; FK to `AspNetRoles` with cascade. No row: built-in defaults. A role made on the page has `AspNetRoles.Scope = custom`. [ACC-001](../../specs/identity/002-access-rights/README.md) |

```mermaid
erDiagram
    AspNetUsers ||--o{ AspNetUserRoles : "UserId FK"
    AspNetRoles ||--o{ AspNetUserRoles : "RoleId FK"
    AspNetUsers ||--o{ AspNetUserClaims : "UserId FK"
    AspNetRoles ||--o{ AspNetRoleClaims : "RoleId FK"
    AspNetUsers ||--o{ AspNetUserLogins : "UserId FK"
    AspNetUsers ||--o{ AspNetUserTokens : "UserId FK"
    AspNetRoles ||--o| role_access : "role_id FK"
```
