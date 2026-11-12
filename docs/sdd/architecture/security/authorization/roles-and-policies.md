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
| `Seo` | Staff («SEO-фахівець», [DESK-001](../../../specs/identity/003-role-desks/README.md)): pages from «Права доступу» | yes |
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
| `CatalogRead` | User, Accountant, Manager, Seo, Admin and every role made on «Права доступу»: shared catalogue reads |
| `OwnProfile` | Accountant, Manager, WorkshopChief, Seo, Admin and every role made on «Права доступу» (`/api/profile`) |
| `OrderCreate` | User, Manager, Admin |
| `RequireTenantAccess` | Constant is defined; policy is not currently registered |
| `access:<page>[:<action>]` | «Права доступу» ([ACC-001](../../../specs/identity/002-access-rights/README.md)): the role's saved rights on the page, or the built-in defaults; the director always. |

### «Права доступу» pages

Every work page of the admin is guarded by `access:<page>`; the action comes from the HTTP method (GET view, POST create, PUT/PATCH edit, DELETE delete), a page without that action asks for edit, and `access:<page>:<action>` or `[AccessAction]` names it when the method says otherwise. Pages and defaults are in `Identity.Domain/Authorization/AccessCatalog.cs`, in the groups and order of the admin menu (`apps/admin-react/src/app/router/legacy-menu.ts` in brevi-web); until a role is saved its defaults are the rules below. Each page offers only the actions it really has: «Створення» and «Видалення» appear where the page creates or deletes something (2026-11-12 audit), and a POST that changes, voids, closes or reopens something is marked `[AccessAction(Edit)]`.

| Page | Endpoints | Default rights |
| --- | --- | --- |
| `desk-main`, `desk-sales`, `desk-workshop`, `desk-seo`, `desk-accounting` | none: which «Робочий стіл» desks the admin home shows ([DESK-001](../../../specs/identity/003-role-desks/README.md)); each block also needs its own page | main: Admin; sales: Admin, Manager; workshop: Admin, WorkshopChief; SEO: Admin, Seo; accounting: Accountant |
| `quotes` | `/api/crm/quotes`, quote introductions and templates; lookups and mailboxes need view; delete and the old-archive purge need `access:quotes:delete` | Admin, Manager: view, create, edit; delete only the director |
| `quote-settings` (group «Адміністрування») | `/api/crm/quote-settings` | Admin |
| `embroidery` | `/api/crm/embroidery` (settings stay Admin); «Розрахувати» needs view, a saved calculation create, its delete delete | Admin, Manager: all |
| `tasks` | `/api/crm/tasks` (task admin stays Admin); create, edit, delete | Admin, Manager: all |
| `leads` | `/api/crm/leads` ([LEAD-001](../../../specs/crm/006-leads/README.md)) | Admin, Manager |
| `lead-stats` (group «Адміністрування», «Статистика лідів») | `/api/crm/lead-stats`; edit = «Довідники», create = «Синхронізувати зараз» | Admin |
| `production-orders`, `fabric-stock`, `forecast`, `workshop-archive` (`fabric-stock`: create = openings, receipts, issues, adjustments; delete = a stock line; reverse, void, restore are edit; `production-orders`: close, cancel, reopen, files and Telegram are edit) | `/api/production/orders`, dashboard, `/api/warehouse/fabric-stock` (group «Склад тканин», [WH-001](../../../specs/warehouse/001-warehouse-module/README.md)), the item tab `/api/warehouse/orders/…/fabric` (`production-orders`), forecast, archive | Admin, Manager, WorkshopChief (rules inside by role, [PRD-002](../../../specs/production/002-production-orders/README.md)–[PRD-008](../../../specs/production/008-workshop-archive/README.md)) |
| `fabric-plan-fact` (group «Загальні звіти»), `fabric-booking` («Що замовити») | `/api/warehouse/reports/plan-fact`, `/booking` ([WH-005](../../../specs/warehouse/005-reports/README.md)) | Admin, Manager, WorkshopChief (view) |
| `fabric-valuation` (group «Загальні звіти») | `/api/warehouse/reports/valuation` | Admin (the owner; view) |
| `hardware-order` | `/api/warehouse/reports/hardware` | Admin, WorkshopChief (view) |
| `work-log`, `workshop-settings` (group «Адміністрування»), `workshop-staff`, `fabric-norms`, `payroll`, `timesheet` (the three «Персонал цеху» tabs) | `/api/production/work-log`, settings, staff, fabric norms and model compositions, payroll, timesheet; create = work entry, worker, payroll payment or adjustment; voids and month close are edit; `fabric-norms` delete = «Видалити норму» | Admin, WorkshopChief: every action the page has |
| `workshop-reports` (group «Загальні звіти») | `/api/production/reports` | Admin (view only) |
| `accounting`, `purchase-invoices` | `/api/accounting/**` (create = new operations, transfers, accounts, categories, counterparties, statement and file uploads; delete = accounts, categories, attachments; void, archive, restore, merge, book and rollback are edit), `/api/purchasing/invoices` | the director; Accountant every action of «Бухгалтерія», view and edit of «Рахунки на закупку» (owner 2026-11-12; a role saved earlier gets them too unless the director unticks them, see [DESK-001](../../../specs/identity/003-role-desks/README.md) D6) ([PUR-001](../../../specs/purchasing/001-purchase-invoices/README.md)) |
| `reminders` | `/api/reminders/**` except the Telegram webhook; create = task, non-working day; delete = archive a task, remove a day; preview is view; close, reopen, chat test are edit | Admin: all |
| `products`, `product-sections`, `materials`, `operations`, `pricing-parameters`, `price-import` | catalogue and reference writes (`materials` covers fabrics, accessories and colours «Кольори»); reads stay `CatalogRead` | Admin, Manager; Accountant view |
| `counterparties` | suppliers, clients and their contacts | Admin, Manager |
| `media` | `/api/media/**` | Admin, Manager; Seo view and upload |
| `content-*` | `/api/content/**` («Контент») | Admin; Manager view; Seo view, create and edit (settings, site pages, «Контакти та компанія»: view and edit; «SEO-огляд»: view) |
| `content-products` | `/api/content/products` ([CNT-002](../../../specs/content/002-product-pages/README.md)) | Admin; Manager view |
| `content-categories` | `/api/content/categories` ([CNT-003](../../../specs/content/003-site-categories/README.md)) | Admin; Manager view |
| `content-articles` | `/api/content/articles` ([CNT-004](../../../specs/content/004-articles/README.md)) | Admin; Manager view |
| `content-redirects` | `/api/content/redirects` ([CNT-005](../../../specs/content/005-redirects/README.md)) | Admin; Manager view |
| `content-seo-settings` | `/api/content/seo-settings` ([CNT-006](../../../specs/content/006-seo-settings/README.md)) | Admin view and edit; Manager view |
| `content-site-pages` | `/api/content/site-pages` ([CNT-007](../../../specs/content/007-site-pages/README.md)) | Admin view and edit; Manager view |
| `content-company` | `/api/content/company` ([CNT-009](../../../specs/content/009-site-company/README.md)) | Admin and Seo view and edit; Manager view |
| `content-seo-overview` | `/api/content/seo-overview` ([CNT-008](../../../specs/content/008-seo-overview/README.md)) | Admin, Manager view (read-only page) |

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
| `GET /api/public/catalog/*` (tree, products, product by address, product addresses) | Catalog of the public site, read-only: only products marked «Показувати на сайті», nothing internal (CNT-002). |
| `GET /api/public/articles`, `GET /api/public/articles/{slug}` | Articles of the public site, read-only: only articles marked «Показувати на сайті» (CNT-004). |
| `GET /api/public/redirects` | Redirects of the public site, read-only: old address and final page, nothing internal (CNT-005). |
| `GET /api/public/site-settings` | Analytics ids, verification codes and the sharing picture of the public site, all visible in its pages anyway (CNT-006). |
| `GET /api/public/company` | Company phones, emails, address, hours, managers, payment details and links, all shown on the site anyway (CNT-009). |
| `GET /api/public/site-pages` | Search titles and descriptions of the fixed site pages, visible in their pages anyway (CNT-007). |

Every other operation needs a signed-in user, and every admin operation names a role or policy: work pages `access:<page>`, shared reads `CatalogRead`, users and system settings `Admin`. The same test fails when an endpoint has neither, except `GET /api/auth/session/me`, `GET /api/auth/session/access` and `POST /api/auth/cookie/logout`, which only need a session; another test fails when a named policy does not exist.

A class-level `[AllowAnonymous]` overrides any `[Authorize]` on its methods, so put it only on the single public action. The exceptions are `PublicCatalogController`, `PublicArticlesController`, `PublicRedirectsController` and `PublicSiteSettingsController`, where every action is public by design.

## New feature or page: «Права доступу» checklist

Every staff page of the admin and every API it calls must be configurable on «Права доступу». The page lists `AccessCatalog.Areas`, so a page missing from the catalogue can't be granted to anyone. Decide the access in the SDD, then do the following in the same change.

1. **Choose the page.** If the feature is a new screen or menu item, add an `AccessArea` to `Identity.Domain/Authorization/AccessCatalog.cs` with:
   - a constant key (kebab-case, stable: it is stored in `identity.role_access`);
   - its group (the admin menu group, in Ukrainian) and a Ukrainian title, in menu order. When a menu item moves to another group, move its `AccessArea` in the same change;
   - only the actions the page really has (`View`, `Create`, `Edit`, `Delete`). When an existing page gains `Create` or `Delete`, add an Identity data migration that gives them to every saved role with `Edit` on it (like `ExpandAccessActions`), because until then edit covered them;
   - defaults for the built-in roles. The defaults decide what people see right after deploy, before anyone saves the page, so give exactly the roles the SDD names.

   If the feature only adds a part of an existing page (a tab, a panel, a button), reuse that page's key and pick the action instead. Add a new key only when the director would want to grant it separately.
2. **Guard every endpoint** with `[Authorize(Policy = "access:<page>")]`. Put the constant in the module's `Api/Access/*Access.cs`, like `CrmAccess` or `ProductionAccess`.
   - The HTTP method gives the action: GET view, POST create, PUT/PATCH edit, DELETE delete.
   - When the method doesn't match the meaning, name the action: `access:<page>:delete` for a delete done by POST, or `[AccessAction(AccessActionAttribute.Edit)]` for a POST that only edits (send, reprice).
   - Never `[Authorize(Roles = …)]` on a work page. Users, «Права доступу» and system settings stay `CanManageUsers`, and shared catalogue reads stay `CatalogRead`.
3. **Keep business rules inside Application.** Rules such as "a manager sees only their own КП" stay in the handler. The page right only decides whether the person may open the page or do the action. Write down in the SDD how a role made on «Права доступу» is treated by those rules: today it is treated like a manager.
4. **Tests:**
   - Update the module's policy assertions in its contract tests.
   - `tests/HostApiTests/ApiAccessTests.cs` must still pass. It fails on an endpoint without a policy or with an unknown one.
   - `AccessRightsTests` covers the catalogue and its defaults.
5. **Docs:**
   - Add the row to the pages table above.
   - In the feature card, state the page key and default rights.
   - In the OpenAPI contract, write `Requires access:<page>` in each operation's description, quoted inside a flow mapping, because a bare `:` breaks YAML there.
6. **Hand off to the frontend:** the admin needs the same key on its menu item and its route, and its actions on buttons. See the brevi-web standard `docs/standards/admin/access-rights.md`.

## Adding a policy or role

1. A new admin page: follow the checklist above.
2. Check whether an existing policy already covers the business operation.
3. If not, define the policy name in Identity Application and its role requirement in Host.Api.
4. If a new built-in role is needed, add its constant in Identity Domain and deliberately configure seeding; define which existing and future users receive it.
5. Protect the endpoint with the new policy, update the API contract, and manually verify denied and allowed requests.

Adding a role is an authorization change, not just a string change: it affects seeding, user assignment, documentation, and deployment.
