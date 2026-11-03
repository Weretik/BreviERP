# Staff Accounts

**Module:** identity.

The Admin «Користувачі» page: an administrator lists administrators and managers, creates them, changes their role, blocks or unblocks them and sets a new password. There is no open registration; the first administrator comes from the seed job ([session flow](../../architecture/security/authentication/session-flow.md#first-administrator)).

## Rules

- Policy `CanManageUsers` (role Admin) on every operation. A manager gets `403`.
- Roles that can be given: `Admin`, `Manager` (`StaffRoles`). Users with only `User` are not listed.
- Password: 8 to 128 characters. Email unique (`409` on a duplicate).
- An administrator cannot change their own role or block themselves (`400`).
- Blocking sets the lockout end to the far future; changing a role, blocking or a new password changes the security stamp, so the user's sessions end within 5 minutes.

## Code structure

```text
src/Modules/Identity/
├── Identity.Api/Identity.Api/
│   ├── Controllers/AdminUsersController.cs          # /api/admin/users
│   └── Contracts/Users/                             # request and response records
├── Identity.Application/Identity.Application/
│   ├── Features/Users/                              # GetStaffList, CreateStaff, ChangeStaffRole,
│   │                                                # SetStaffBlocked, SetStaffPassword + validators
│   └── Services/IStaffAccountService.cs
└── Identity.Infrastructure/Identity.Infrastructure/Services/Users/StaffAccountService.cs
```

## Tests

`tests/IntegrationTests/Identity/AdminUsersApiTests.cs`: create a manager who signs in but gets `403` on the users API, short password and duplicate email, block and self-protection, new password and lockout after 5 wrong passwords. Focused run: `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter FullyQualifiedName~AdminUsersApiTests`.

## Related sources

[OpenAPI](../../contracts/identity/users.openapi.yaml), [roles and policies](../../architecture/security/authorization/roles-and-policies.md).
