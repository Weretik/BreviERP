# ACC-001 «Права доступу»

**Status:** implemented 2026-11-08. **Module:** Identity (table `identity.role_access`). **Consumer:** brevi-web «Адміністрування → Права доступу», the menu and buttons of the whole admin. **Source:** client corrections 2026-11-08, item 11.

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

## Contract and tests

[OpenAPI](../../../contracts/identity/access.openapi.yaml): `getAccessRights`, `saveRoleAccess`, `createRole`, `deleteRole`; `getSessionAccess` in the [session contract](../../../contracts/identity/session.openapi.yaml). Tests: `tests/UnitTests/Identity/AccessRightsTests.cs`, `tests/HostApiTests/AccessRequirementHandlerTests.cs`, `tests/HostApiTests/ApiAccessTests.cs`, `tests/IntegrationTests/Identity/AccessRightsApiTests.cs`.

## Deployment

Migration `AddRoleAccess` (Identity) runs with the normal deploy; the new table starts empty, so every role keeps its current rights. Rollback: the previous image ignores the table.
