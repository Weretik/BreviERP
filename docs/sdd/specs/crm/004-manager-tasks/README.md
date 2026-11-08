# TSK-001 — Manager tasks and kanban

**Status:** stage 1 (backend) implemented 2026-11-04; Admin stages 2–4 in brevi-web. **Date:** 2026-11-04. **Module:** Crm («Менеджер сервісу»), schema `crm`, migration history `crm.__EFMigrationsHistory` ([module rules](../../../standards/module-rules.md)). Feature card: [manager tasks](../../../features/crm/manager-tasks.md). Contract: [tasks.openapi.yaml](../../../contracts/crm/tasks.openapi.yaml).

## Why

The product owner asked on 2026-11-04 (project thread «Add manager tasks and kanban to Brevi») for tasks that Director or Admin give to managers and track, and for a kanban of those tasks in «Менеджер сервісу». The owner chose the designs: in «Адміністрування» a task list with a side panel and the workload of each manager (A1 + workload from A2), and in «Менеджер сервісу» a board with the lanes «Прострочено · Терміново · У роботі · Виконано» (K2). Director and Admin see every manager's tasks on that board, switch with a manager filter and edit any task there.

## Decision: inside Crm, not a new module

Tasks are work of the «Менеджер сервісу» block and may point to a Crm client, so they live in Crm next to clients and КП (owner, 2026-11-04). The «Задачі» of Reminders are scheduled Telegram reminders and stay separate.

## Rules

- **R-01:** Director and Admin (the Director also holds Admin) see all tasks. A Manager sees only tasks given to them; a foreign task is «not found».
- **R-02:** Only Director and Admin create, edit, give to another manager, mark urgent and delete tasks.
- **R-03:** A task has: what to do (required, up to 200), description (up to 4000), manager (required), due date and time (required), «Терміново» yes/no, an optional active Crm client, and up to 30 steps (up to 300 each). The number «ЗД-0142» is the id with a prefix.
- **R-04:** A task can be given only to an active (not blocked) user with the Manager role.
- **R-05:** Status is what the manager did: `New`, `InProgress` or `Done`. «Прострочено» and «Терміново» are not statuses.
- **R-06:** Lane (kanban column): `Done` when done; otherwise `Overdue` when the due time has passed; otherwise `Urgent` when marked urgent; otherwise `Active` («У роботі»). An urgent task past its due time is overdue (owner's question 2026-11-04, proposed default). The board shows every open task and the tasks done in the chosen period (`period`: «Цей тиждень» from Monday in Kyiv by default, «Минулий тиждень», 30, 90 or 365 days, or all), newest added first in every column (client corrections round 2, items 10 and 11, 2026-11-08).
- **R-07:** The manager (or Director/Admin) changes the status. `Done` stores the completion time; leaving `Done` clears it. Setting the same status changes nothing.
- **R-08:** Dragging a card into or out of «Терміново» is a Director/Admin change of the urgent mark.
- **R-09:** The overview gives counts for the metric strip and chips (open, overdue, urgent, new, in progress, done, done in the last 7 days, the share done on time in the last 30 days) and the workload of every active manager (open, overdue, urgent, in progress, done in the last 7 days, the share done on time in the last 30 days) for the «Навантаження» tab, including managers with no tasks.
- **R-10:** The manager (or Director/Admin) ticks and unticks steps.
- **R-11:** «Історія змін» records who created, edited, reassigned, changed the due time, status, urgency or a step, and when; the client text is the Admin's.
- **R-12:** Every change checks the version the screen loaded; a stale change is rejected with 409.

Later, only on the owner's request: a Telegram message to the manager through the existing bot when a task is given or becomes overdue.

## Scenarios

| ID | Given | When | Then |
| --- | --- | --- | --- |
| SC-01 | Director, manager Ольга | Creates a task with two steps for Ольга | `ЗД-0001`, status `New`, lane `Active`, steps in order, history «Created» |
| SC-02 | Director | Gives a task to himself, or links an unknown client; a Manager tries to create | 400, 400, 403 |
| SC-03 | Tasks of Ольга and Ірина | Ольга lists with Ірина's filter; Ірина changes Ольга's task | Only Ольга's tasks; 404 |
| SC-04 | Urgent task due in 1 h, urgent in 48 h, ordinary, done | 2 h later the board loads | Lanes Overdue, Urgent, Active, Done; overdue first in the list |
| SC-05 | Task done 8 days ago | Board loads | Not on the board |
| SC-06 | Two windows | Director edits in both | Second save 409; reassigned task disappears for the old manager |
| SC-07 | Task with a step | Manager ticks it; Manager marks urgent; Director marks urgent; Director deletes | Step done; 403; lane Urgent; gone |
| SC-08 | Three managers, tasks of two | Director opens the overview; a Manager passes another manager's filter | Workload for all three with zeros; Manager gets own counts only |

Evidence: `tests/IntegrationTests/Crm/ManagerTaskPersistenceTests.cs` (SC-01…SC-08), `tests/UnitTests/Crm/Tasks/`.

## Data

Schema `crm`, migration `AddManagerTasks`, see [Crm schema](../../../architecture/data-model/crm.md).

| Table | Key columns |
| --- | --- |
| `ManagerTasks` | `Id` (bigint identity), `Title`, `Description`, `AssigneeUserId`, `AssigneeName`, `AuthorUserId`, `AuthorName`, `DueAt`, `IsUrgent`, `Status`, `CustomerPublicId`, `CustomerName`, `CompletedAt` (set only when `Done`, check constraint), `CreatedAt`, `UpdatedAt`, `Version` (concurrency) |
| `ManagerTaskSteps` | `Id`, `TaskId` (cascade), `Position`, `Text`, `IsDone` |
| `ManagerTaskEvents` | `Id`, `TaskId` (cascade), `Kind`, `ByUserId`, `ByName`, `At`, `Details` |

Identity users are stored as ids with names, without foreign keys. The client is its public id with the name at the time of saving, without a foreign key: the KeepinCRM sync may delete clients and the task keeps the name. Rollback: the previous image ignores the new tables.

## Admin stages (brevi-web)

1. «Адміністрування › Завдання менеджерам»: tabs «Завдання» (metric strip, search, chips, table, side panel) and «Навантаження» (table of managers, L3 chosen 2026-11-04).
2. «Менеджер сервісу › Мої завдання»: board K2 with «На сьогодні»; Director/Admin get the manager filter and the same side panel.
3. The first menu item becomes the «Робочий стіл» tile (H2).
