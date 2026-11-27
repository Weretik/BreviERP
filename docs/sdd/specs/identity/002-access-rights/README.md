# ACC-001 «Права доступу»

**Status:** implemented 2026-11-08; redesign ACC-002 2026-11-27. **Module:** Identity (table `identity.role_access`). **Consumer:** brevi-web «Адміністрування → Права доступу», the menu and buttons of the whole admin. **Source:** client corrections 2026-11-08, item 11.

## Goal

The director and administrators decide, per role, which admin pages it sees and what it may do there (see, create, edit, delete), and add or remove roles, without a developer. The server checks the same rights on every request; the admin hides menu items and buttons by them.

## Decisions

| ID | Decision |
| --- | --- |
| D1 | Pages are a fixed catalogue in code (`AccessCatalog`): 27 pages in 6 groups in menu order, each with the actions it has (`View`, `Create`, `Edit`, `Delete`). Admin-only system pages («Користувачі», «Права доступу», «Налаштування ШІ», imports) are not in it and stay `Admin`. |
| D2 | The director always has every right, also on pages added later; their rights are not edited (400). |
| D3 | Built-in roles (Director, Admin, Manager, Accountant, WorkshopChief) keep the rules they had before the page existed until someone saves other rights; nothing changes on deploy. They are never deleted (400). |
| D4 | A new role is an Identity role with scope `custom`, a generated name `custom-…` and the given title (1–100 characters, unique, 409 on a duplicate). It starts with no rights. It can be given to staff on «Користувачі». A role with users is not deleted (409). |
| D5 | Saved rights are one row per role in `identity.role_access`: page → action flags as JSON, a version and who saved it. A save sends the version it loaded; a newer save in between returns 409. Saving an action needs «Перегляд» on the same page. |
| D6 | Endpoints name a page with policy `access:<page>`; the action comes from the HTTP method (GET view, POST create, PUT/PATCH edit, DELETE delete); a page without that action asks for «Редагування», a view-only page for «Перегляд». `access:<page>:<action>` or `[AccessAction]` names it when the method says otherwise (КП delete, reprice, send). A user with several roles gets every right any of them has. |
| D7 | Rights are read once per 30 seconds per API instance; a save or delete clears them on the instance that made it, other instances follow within 30 seconds. Role claims come from the session cookie, so a changed role still waits for the 5-minute stamp check. |
| D8 | Shared reads stay open to staff: `CatalogRead` (catalogue pickers in КП, workshop and reference pages) and `OwnProfile` («Мій кабінет») admit every built-in staff role as before and every new role. |
| D9 | Business rules inside pages keep their role checks: a manager sees only own КП and production orders, «бачить усі» stays Admin, workshop stage rules stay WorkshopChief and Admin. A new role behaves like a manager there. |
| D10 | Access to the page itself: `CanManageUsers` (Admin; the director also holds Admin). |

## Redesign 2026-11-27 (ACC-002)

**Source:** owner request 2026-11-27: a new «Права доступу» page (concept Б: roles on the left, the chosen role's rights on the right) and an audit of what the page misses. The audit found switches that did nothing (the server still asked for the Admin role) and pages that could not be granted at all.

| ID | Decision |
| --- | --- |
| R-01 | «Мої завдання» (`tasks`) offers only «Перегляд» and «Редагування» (status and steps), because creating, changing, «Терміново» and deleting a task were Admin-only on the server. Those actions and the assignee list are the new page `manager-tasks` «Завдання менеджерам» (group «Адміністрування», every action; default Admin). Its «Перегляд» also means «sees every manager's tasks» (was the Admin role, TSK-001 R-01). The task reads stay on `tasks`, so the page works together with «Мої завдання: Перегляд». |
| R-02 | «Ціна вишивки» (`embroidery`): deleting a saved calculation needs its «Видалення» (was the Admin role). Cost, rates and «Налаштування» are the new page `embroidery-settings` «Ціна вишивки: собівартість і налаштування» (group «Менеджер сервісу», view and edit; default Admin); its «Перегляд» is what EMB-001 R-02 called «sees the cost». |
| R-03 | Product photos are changed with «Товари: Редагування» (was `RequireAdminRole`); a PPE price source with «Імпорт цін: Редагування», and the list's `canEdit` follows that right. |
| R-04 | Some actions stay Admin-only on purpose: one-time imports of old data (КП, embroidery archive, old site photos), merging two clients and giving a КП to another manager (both need «бачить усі», D9), and the system pages «Користувачі», «Права доступу», «Налаштування ШІ». `AccessCatalog.AdminOnly` lists them; the page shows them as a block without switches, so the director sees the whole admin. |
| R-05 | Every page carries a Ukrainian name for each of its actions (`AccessActionLabels`), such as «Запускати зараз» for «Фонові задачі: Створення»; a page without its own name for an action shows the general one («Бачить», «Створювати», «Редагувати», «Видаляти»). `GET /api/admin/access` returns them as `actionLabels`. |
| R-06 | Each role in `GET /api/admin/access` lists the names of the people who hold it (`userNames`, director counted only as the director), for the role header. |
| R-07 | A role made on the page can be renamed: `PUT /api/admin/access/roles/{role}/title` (`renameRole`). A built-in role: 400; an unknown or built-in-scope role: 404; a title another role has: 409; the role's concurrency stamp guards a parallel change (409). Renaming to the same title again changes nothing (idempotent without a `requestId`). Rate limit: the global soft limit, a cheap signed-in admin call. |
| R-08 | The new pages use `DefaultsForSavedRoles`, so a role saved before them (for example a saved Admin) keeps managing tasks and seeing the embroidery cost after the deploy. No data migration: `tasks` only loses actions that never worked. |
| R-09 | «Копіювати з ролі» needs nothing new on the server: the page copies the other role's rights into its draft and saves them with `saveRoleAccess`. |

## Contract and tests

[OpenAPI](../../../contracts/openapi.json): `getAccessRights`, `saveRoleAccess`, `createRole`, `renameRole`, `deleteRole`; `getSessionAccess` in the [session contract](../../../contracts/openapi.json). Tests: `tests/UnitTests/Identity/AccessRightsTests.cs`, `tests/HostApiTests/AccessRequirementHandlerTests.cs`, `tests/HostApiTests/ApiAccessTests.cs`, `tests/IntegrationTests/Identity/AccessRightsApiTests.cs`.

## Deployment

Migration `AddRoleAccess` (Identity) runs with the normal deploy; the new table starts empty, so every role keeps its current rights. Rollback: the previous image ignores the table.
