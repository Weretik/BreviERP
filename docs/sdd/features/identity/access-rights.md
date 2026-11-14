# Access Rights

**Module:** identity. **Specification:** [ACC-001](../../specs/identity/002-access-rights/README.md).

«Адміністрування → Права доступу»: per role, which admin pages it sees and what it may do there; new roles are added and removed here. Every work page's API checks these rights; the admin reads the user's own rights from `GET /api/auth/session/access` to hide menu items and buttons.

## Rules

- Pages and their actions: `AccessCatalog` (Identity Domain). The director always has everything and is not edited; built-in roles keep their old rules until saved; a new role starts empty.
- Endpoints use `access:<page>` (action from the HTTP method) or `access:<page>:<action>`; `[AccessAction]` overrides the method's action. `AccessPolicyProvider` builds these policies, `AccessRequirementHandler` checks them (Host.Api).
- Saved rights: `identity.role_access`, versioned (409 on a stale save), cached 30 seconds per instance.
- A role with users is not deleted (409); built-in roles are never deleted (400).
- Built-in roles: Director, Admin, Manager, Accountant, WorkshopChief and Seo («SEO-фахівець»: «Контент» without delete, photos view and upload).
- Group «Робочі столи» (`desk-*`, [DESK-001](../../specs/identity/003-role-desks/README.md)) has no API: it tells the admin home which desks a role sees.

## Code structure

```text
src/Modules/Identity/
├── Identity.Domain/Identity.Domain/Authorization/AccessCatalog.cs   # pages, actions, defaults, role helpers
├── Identity.Application/Identity.Application/
│   ├── Features/Access/{Commands,Queries}/      # GetAccessRights, SaveRoleAccess, CreateRole, DeleteRole,
│   │                                            # GetMyAccess + validators, Shared DTOs
│   ├── Security/Access/                         # AccessPolicies, IAccessGrantSource, AccessClaims, AccessCheck,
│   │                                            # AccessRoles, DefaultAccessGrantSource (catalogue defaults)
│   └── Abstractions/IAccessRightsService.cs
├── Identity.Infrastructure/Identity.Infrastructure/
│   ├── Persistence/Access/RoleAccess.cs, Persistence/Access/RoleAccessConfiguration.cs
│   └── Integrations/AspNetIdentity/Access/      # AccessRightsService, AccessGrantSource (cached)
└── Identity.Api/Identity.Api/
    ├── Endpoints/AccessRights/                  # /api/admin/access, one <UseCase>/ folder each
    ├── Endpoints/Session/GetSessionAccess/ # GET /api/auth/session/access
    ├── Access/AccessContracts.cs
    ├── Access/                                  # AccessPolicyProvider, AccessRequirementHandler (asks AccessCheck)
    └── DependencyInjection/IdentityAuthorizationExtensions.cs # named policies, AddAccessPolicies
src/BuildingBlocks/BuildingBlocks.Api/Authorization/AccessActionAttribute.cs
```

Module access classes hold the policy strings: `CrmAccess`, `ProductionAccess`, `AccountingAccess`, `PurchasingAccess`, `MediaAccess`, `RemindersAccess`; Catalog and Reference controllers write `access:<page>` directly.

## Tests

`dotnet test tests/UnitTests/UnitTests.csproj --filter AccessRightsTests`, `dotnet test tests/HostApiTests/HostApiTests.csproj` (every named policy exists, defaults deny as before, the handler), `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter AccessRightsApiTests` (real database).

## Related sources

[OpenAPI](../../contracts/identity/access.openapi.yaml), [roles and policies](../../architecture/security/authorization/roles-and-policies.md), [staff accounts](staff-accounts.md).
