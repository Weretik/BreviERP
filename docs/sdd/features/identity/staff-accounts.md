# Staff Accounts

**Module:** identity.

The Admin «Користувачі» page: an administrator or the director lists staff accounts (director, administrators, accountants, managers), creates them, changes their name, email and phone, changes their role, blocks or unblocks them, sets a new password and deletes them. The list shows each account's photo and phone. There is no open registration; the first administrator comes from the seed job ([session flow](../../architecture/security/authentication/session-flow.md#first-administrator)).

## Rules

- Policy `CanManageUsers` (role Admin) on every operation. The director also holds Admin. A manager or accountant gets `403`.
- Roles that can be given: `Director`, `Admin`, `Accountant`, `Manager`, `WorkshopChief`, `Seo` (`StaffRoles`) and roles made on [«Права доступу»](access-rights.md) (an unknown role returns `400`). A director account gets both `Director` and `Admin`; the list shows the strongest role. Users with only `User` are not listed.
- At most one director: creating or promoting a second one returns `409`.
- Only the director changes the director account (role, block, password, delete); an administrator gets `403`.
- Nobody blocks or deletes their own account (`400`). Only the director may change their own role (`400` for an administrator).
- Password: 8 to 128 characters. Email unique (`409` on a duplicate). A new email ends the account's sessions.
- Blocking sets the lockout end to the far future; changing a role, blocking or a new password changes the security stamp, so the user's sessions end within 5 minutes. Delete removes the account.

## Code structure

```text
src/Modules/Identity/
├── Identity.Api/Identity.Api/
│   ├── Controllers/AdminUsersController.cs          # /api/admin/users
│   └── Contracts/Users/                             # request and response records
├── Identity.Application/Identity.Application/
│   ├── Features/Users/                              # GetStaffList, CreateStaff, ChangeStaffRole,
│   │                                                # SetStaffBlocked, SetStaffPassword, DeleteStaff,
│   │                                                # UpdateStaffDetails
│   │                                                # + validators
│   └── Services/IStaffAccountService.cs
└── Identity.Infrastructure/Identity.Infrastructure/Services/Users/StaffAccountService.cs
```

## Tests

`tests/IntegrationTests/Identity/AdminUsersApiTests.cs`: create a manager who signs in but gets `403` on the users API, short password and duplicate email, block and self-protection, new password and lockout after 5 wrong passwords, one director whom only the director changes, delete but not self-delete. Focused run: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~AdminUsersApiTests`.

## Related sources

[OpenAPI](../../contracts/identity/users.openapi.yaml), [roles and policies](../../architecture/security/authorization/roles-and-policies.md).
