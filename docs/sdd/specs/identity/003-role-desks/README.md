# DESK-001 Role «SEO-фахівець» and «Робочі столи»

**Status:** all 5 stages implemented 2026-11-11. **Module:** Identity (`AccessCatalog`, roles; no schema change). **Consumer:** brevi-web «Робочий стіл» (admin home `/`), «Користувачі», «Права доступу». **Source:** owner request 2026-11-11, [concept](https://claude.ai/artifact/BABr1LD9V1954wsa6w4Uip).

## Goal

Every built-in role opens its own «Робочий стіл» with the blocks of its work; the director and administrators see their main desk and every other desk as blocks they can fold. A built-in role «SEO-фахівець» works on the site texts and search settings. The owner creates its accounts on «Користувачі».

## Decisions

| ID | Decision |
| --- | --- |
| D1 | New built-in role `Seo` (title «SEO-фахівець», scope `content`), seeded by `RoleSeeder` on the next start; it can be given on «Користувачі», never deleted, and its rights are edited on «Права доступу» like any built-in role. |
| D2 | `Seo` defaults: every «Контент» page view, create and edit, never delete (owner proposal accepted 2026-11-11); «Налаштування SEO сайту» and «Сторінки сайту» view and edit; «SEO-огляд» view; «Бібліотека фото» view and upload. No КП, workshop, warehouse or money. `CatalogRead` and `OwnProfile` admit it. |
| D3 | A new group «Робочі столи» in «Права доступу» with five view-only pages: `desk-main` (Головний стіл директора), `desk-sales` (Стіл менеджера), `desk-workshop` (Стіл цеху), `desk-seo` (Стіл SEO), `desk-accounting` (Стіл бухгалтера). They have no API; the admin home shows the desks the user may see. |
| D4 | Desk defaults: main Admin; sales Admin, Manager; workshop Admin, WorkshopChief; SEO Admin, Seo; accounting Accountant. The director sees all. Any role, also a role made on «Права доступу», can be given any desk. |
| D5 | A block inside a desk reads an existing endpoint and is shown only with view on that block's own page (for example money needs `accounting`, which Admin does not have by default). No new endpoint in stage 1. |
| D6 | Desk pages reach roles saved before them (`AccessArea.DefaultsForSavedRoles`): a saved row that does not mention a desk gets the role's default, and every save writes each desk explicitly (0 when unticked), so nobody loses the home page on deploy and the director's later choice wins. Other pages keep the old rule (a saved role gets a new page only when ticked). |
| D7 | No role for seamstresses: they do not sign in; the workshop desk serves the head of the workshop (owner, 2026-11-11). |
| D9 | Stages 3–4 (SEO, accountant) read existing endpoints only. Stage 5: the main desk of the director and administrators shows a strip of key figures, «Потребує уваги» (problems of every desk the person may see, one list) and every other visible desk as a foldable block below. The one new read is «КП за місяць» (`GET /api/crm/quotes/period`): КП created and sent in a Kyiv period, in total and per responsible person; a КП sent twice counts once; a Manager counts only their own. Money stays behind `accounting`. |
| D10 | Owner 2026-11-12: the Accountant gets `accounting` and `purchase-invoices` (view and edit) by default, so the accountant desk works without a setting. Both pages use `DefaultsForSavedRoles` like the desks, so an Accountant row saved earlier gets them; a later save writes them explicitly, so the director can still untick them. |
| D8 | The manager desk reads existing endpoints: «Мій день» of leads, the task overview and open tasks of the signed-in person, their КП (`responsibleUserId`), their open production orders (`managerUserId`, nearest due date first). Only overdue or urgent tasks and late orders are marked. |

## Stages

| Stage | Scope | Status |
| --- | --- | --- |
| 1 | Role `Seo`, group «Робочі столи»; admin: desk frame, workshop desk (old «Огляд виробництва» without КП, with fabric and hardware shortages) | implemented 2026-11-11 |
| 2 | Manager desk; `GET /api/production/orders?managerUserId=` keeps the orders of one manager (the КП responsible) | implemented 2026-11-11 |
| 3 | SEO desk («SEO-огляд» on the desk) | implemented 2026-11-11 (admin only) |
| 4 | Accountant desk | implemented 2026-11-11 (admin only) |
| 5 | Director and admin main desk; every desk as a foldable block; `GET /api/crm/quotes/period` «КП за місяць» | implemented 2026-11-11 |

## Contract and tests

Stage 5 adds `getCrmQuotePeriod` to the [quotes contract](../../../contracts/crm/quotes.openapi.yaml); `StaffRole`, session roles and `OwnProfile` descriptions name `Seo` in the [users](../../../contracts/identity/users.openapi.yaml), [session](../../../contracts/identity/session.openapi.yaml) and [profile](../../../contracts/identity/profile.openapi.yaml) contracts. Tests: `tests/UnitTests/Identity/AccessRightsTests.cs` (Seo defaults, desks per role, desks for roles saved earlier), `tests/UnitTests/Crm/Quotes/QuotePeriodTests.cs` (period totals per manager, period validation), `tests/HostApiTests`.

## Deployment

No migration. `RoleSeeder` adds the `Seo` role on the next start; roles with saved rights get their default desk (D6) and keep everything else. Rollback: the previous image ignores the unused role and drops the unknown `desk-*` keys when it reads saved rights.
