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
| `Accountant` | Staff («Бухгалтер»): pages from «Права доступу» | yes |
| `Manager` | Staff («Менеджер»): pages from «Права доступу» | yes |
| `WorkshopChief` | Staff («Начальник цеху»): pages from «Права доступу» | yes |
| `custom-…` | Roles made on «Права доступу» (`AspNetRoles.Scope = custom`, title in `Description`); deleted there when nobody has them | no |
| `User` | Regular authenticated user | yes |
| `Guest` | Role constant defined without an agreed business meaning | no |

At startup, `RoleSeeder` creates every missing role except `Guest`, so existing databases gain new roles on the next seed run. Do not assign `Guest` until its seeding and business meaning are deliberately added.

Staff account rules (`StaffAccountService`): there is at most one director (409); only the director may block, delete, reset the password of, or change the role of the director account (403); nobody blocks or deletes their own account, and only the director may change their own role (400).

## Policies

Policy names are in `Identity.Application/Security/Policies/PolicyNames.cs`; their role requirements are composed in `Host.Api/AuthorizationPoliciesRegistrationExtensions`.

| Policy | Allowed roles |
| --- | --- |
| `RequireAdminRole` / `CanManageUsers` | Admin: «Користувачі», «Права доступу», «Налаштування ШІ» ([AI-001](../../../specs/ai/001-ai-settings/README.md)), imports |
| `RequireManagerRole` / `CanManageOrders` | Manager, Admin (registered, not used by Brevi pages) |
| `CatalogRead` | User, Accountant, Manager, Admin and every role made on «Права доступу»: shared catalogue reads |
| `OwnProfile` | Accountant, Manager, WorkshopChief, Admin and every role made on «Права доступу» (`/api/profile`) |
| `OrderCreate` | User, Manager, Admin |
| `RequireTenantAccess` | Constant is defined; policy is not currently registered |
| `access:<page>[:<action>]` | «Права доступу» ([ACC-001](../../../specs/identity/002-access-rights/README.md)): the role's saved rights on the page, or the built-in defaults; the director always. |

### «Права доступу» pages

Every work page of the admin is guarded by `access:<page>`; the action comes from the HTTP method (GET view, POST create, PUT/PATCH edit, DELETE delete), a page without that action asks for edit, and `access:<page>:<action>` or `[AccessAction]` names it when the method says otherwise. Pages and defaults are in `Identity.Domain/Authorization/AccessCatalog.cs`; until a role is saved its defaults are the rules below.

| Page | Endpoints | Default rights |
| --- | --- | --- |
| `quotes` | `/api/crm/quotes`, quote introductions and templates; lookups and mailboxes need view; delete and the old-archive purge need `access:quotes:delete` | Admin, Manager: view, create, edit; delete only the director |
| `quote-settings` | `/api/crm/quote-settings` | Admin |
| `embroidery` | `/api/crm/embroidery` (settings stay Admin) | Admin, Manager |
| `tasks` | `/api/crm/tasks` (task admin stays Admin) | Admin, Manager |
| `production-orders`, `fabric-stock`, `forecast`, `workshop-archive` | `/api/production/orders`, dashboard, fabric stock, forecast, archive | Admin, Manager, WorkshopChief (rules inside by role, [PRD-002](../../../specs/production/002-production-orders/README.md)–[PRD-008](../../../specs/production/008-workshop-archive/README.md)) |
| `work-log`, `workshop-settings`, `workshop-staff`, `fabric-norms`, `payroll-rules`, `payroll`, `timesheet` | `/api/production/work-log`, settings, staff, fabric norms, payroll parameters, payroll, timesheet | Admin, WorkshopChief |
| `workshop-reports` | `/api/production/reports` | Admin (view only) |
| `accounting`, `purchase-invoices` | `/api/accounting/**`, `/api/purchasing/invoices` | only the director ([PUR-001](../../../specs/purchasing/001-purchase-invoices/README.md)) |
| `reminders` | `/api/reminders/**` except the Telegram webhook | Admin |
| `products`, `product-sections`, `materials`, `operations`, `pricing-parameters`, `price-import` | catalogue and reference writes; reads stay `CatalogRead` | Admin, Manager; Accountant view |
| `counterparties` | suppliers, clients and their contacts | Admin, Manager |
| `media` | `/api/media/**` | Admin, Manager |

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

Every other operation needs a signed-in user, and every admin operation names a role or policy: work pages `access:<page>`, shared reads `CatalogRead`, users and system settings `Admin`. The same test fails when an endpoint has neither, except `GET /api/auth/session/me`, `GET /api/auth/session/access` and `POST /api/auth/cookie/logout`, which only need a session; another test fails when a named policy does not exist.

A class-level `[AllowAnonymous]` overrides any `[Authorize]` on its methods, so put it only on the single public action.

## Adding a policy or role

1. A new admin page: add it to `AccessCatalog` with its actions and defaults, and guard its endpoints with `access:<page>`; the admin menu reads the same key.
2. Check whether an existing policy already covers the business operation.
3. If not, define the policy name in Identity Application and its role requirement in Host.Api.
4. If a new built-in role is needed, add its constant in Identity Domain and deliberately configure seeding; define which existing and future users receive it.
5. Protect the endpoint with the new policy, update the API contract, and manually verify denied and allowed requests.

Adding a role is an authorization change, not just a string change: it affects seeding, user assignment, documentation, and deployment.
