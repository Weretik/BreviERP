# Authorization

## Baseline rule

`Host.Api` configures a fallback authorization policy requiring an authenticated user. An endpoint is therefore protected unless it explicitly declares `[AllowAnonymous]`. Anonymous access must be explicit in the API contract and verified as part of the feature.

Authentication answers **who is calling**. Authorization answers **whether that caller may perform the operation**.

## Roles

Canonical role constants are in `Identity.Domain/Authorization/RoleNames.cs`; inherited code also contains `Identity.Domain/Constants/AppRoles.cs` with the same set of values.

| Role | Current meaning | Created by seeding |
| --- | --- | --- |
| `Director` | Owner («Директор»). At most one; the account also holds `Admin`, so every Admin endpoint admits it. Only the director changes the director account. | yes |
| `Admin` | System administration, including «Користувачі» | yes |
| `Accountant` | Staff («Бухгалтер»): signs in and reads the catalogue (`CatalogRead`) for now | yes |
| `Manager` | Staff: everything in Admin except the «Користувачі» page | yes |
| `WorkshopChief` | Staff («Начальник цеху»): the «Швейний цех» block (`CanManageWorkshop`) and «Мій кабінет» | yes |
| `User` | Regular authenticated user | yes |
| `Guest` | Role constant defined without an agreed business meaning | no |

At startup, `RoleSeeder` creates every missing role except `Guest`, so existing databases gain new roles on the next seed run. Do not assign `Guest` until its seeding and business meaning are deliberately added.

Staff account rules (`StaffAccountService`): there is at most one director (409); only the director may block, delete, reset the password of, or change the role of the director account (403); nobody blocks or deletes their own account, and only the director may change their own role (400).

## Policies

Policy names are in `Identity.Application/Security/Policies/PolicyNames.cs`; their role requirements are composed in `Host.Api/AuthorizationPoliciesRegistrationExtensions`.

| Policy | Allowed roles |
| --- | --- |
| `RequireAdminRole` / `CanManageUsers` | Admin |
| `RequireManagerRole` / `CanManageProducts` / `CanManageOrders` | Manager, Admin |
| `CatalogRead` | User, Accountant, Manager, Admin |
| `OwnProfile` | Accountant, Manager, WorkshopChief, Admin (`/api/profile`) |
| `CanManageWorkshop` | WorkshopChief, Admin (`/api/production` references: staff, fabric norms, payroll parameters) |
| `CanUseProductionOrders` | Manager, WorkshopChief, Admin (`/api/production/orders`; rules inside by role, [PRD-002](../../../specs/production/002-production-orders/README.md)) |
| `OrderCreate` | User, Manager, Admin |
| `RequireTenantAccess` | Constant is defined; policy is not currently registered |

Do not use `RequireTenantAccess` on an endpoint until it is registered in Host and its policy semantics are agreed.

## Protecting an endpoint

Prefer an existing named policy that expresses the operation. The controller declares access only; the command or query still goes through `ISender`, and business invariants remain in Domain/Application. Do not use a role check in a controller instead of a policy.

For a public endpoint, use `[AllowAnonymous]` and record the reason in the feature contract. This overrides the fallback policy.

## Current anonymous exceptions

Only these operations are public; `tests/HostApiTests/ApiAccessTests.cs` fails when the list changes:

| Operation | Why |
| --- | --- |
| `POST /api/auth/cookie/login` | Sign-in (rate-limited). |
| `POST /api/reminders/telegram/webhook` | Telegram calls it; checked by the webhook secret header. |
| `GET /api/crm/mailboxes/google-callback` | Google returns the browser here after Gmail consent; accepted only with the signed, 15-minute `state` issued by `POST /api/crm/mailboxes/{userId}/connect`, and it only redirects to the admin. |
| `GET /api/reference/product-categories/store/{language}` | Public storefront categories, read-only. |

Every other operation needs a signed-in user, and every admin operation names a role or policy: reads `CatalogRead` (or a role), writes `CanManageProducts`, CRM `Admin,Manager`, reminders and users `Admin`. The same test fails when an endpoint has neither, except `GET /api/auth/session/me` and `POST /api/auth/cookie/logout`, which only need a session.

A class-level `[AllowAnonymous]` overrides any `[Authorize]` on its methods, so put it only on the single public action.

## Adding a policy or role

1. Check whether an existing policy already covers the business operation.
2. If not, define the policy name in Identity Application and its role requirement in Host.Api.
3. If a new role is needed, add its constant in Identity Domain and deliberately configure seeding; define which existing and future users receive it.
4. Protect the endpoint with the new policy, update the API contract, and manually verify denied and allowed requests.

Adding a role is an authorization change, not just a string change: it affects seeding, user assignment, documentation, and deployment.
