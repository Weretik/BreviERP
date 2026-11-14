# «Менеджер сервісу»: manager tasks

**Implemented:** backend of [TSK-001](../../specs/crm/004-manager-tasks/README.md) (2026-11-04). Director and Admin give tasks to managers, edit, reassign, mark urgent and delete them, and see every manager's tasks and workload (open, overdue, urgent, in progress, done in 7 days, % on time in 30 days). A Manager sees only own tasks, changes their status (`New`, `InProgress`, `Done`) and ticks steps. Lanes for the kanban are computed: overdue, urgent, active, done. The board shows every open task and the tasks done in a chosen period (default this Kyiv week), newest added first. Every change checks the loaded version and is written to the task history.

## Code structure

```text
src/Modules/Crm/
  Crm.Domain/Crm.Domain/Tasks/
    ManagerTask.cs                 root: draft, edit, status, urgency, steps, lane, number, history
    ManagerTaskStep.cs, ManagerTaskEvent.cs, ManagerTaskDraft.cs, ManagerTaskPerson.cs
    ManagerTaskStatus.cs, ManagerTaskLane.cs, ManagerTaskEventKind.cs
    ManagerTaskLimits.cs
  Crm.Application/Crm.Application/Abstractions/Tasks/   ITaskStore, ITaskStaffReader (assignees, Manager role; Crm.Infrastructure/Adapters/Identity)
  Crm.Application/Crm.Application/Features/Tasks/
    Commands/                      CreateTask EditTask ChangeTaskStatus SetTaskUrgent MarkTaskStep DeleteTask
    Queries/                       GetTasks GetTaskBoard GetTaskOverview GetTask GetTaskAssignees
    Shared/Access/                 TaskActor (Admin sees all), Ukrainian messages
    Shared/Persistence/            load/visible/save helpers
    Shared/Building/TaskDraftResolver.cs  side panel → domain draft (assignee, client)
    Shared/                        responses, Mapping/, Validation/
  Crm.Infrastructure/Crm.Infrastructure/Persistence/
    Tasks/                         three tables, cascade steps and events
    CrmDbContext.cs                implements ITaskStore
  Crm.Api/Crm.Api/Tasks/
    ManagerTasksController.cs      /api/crm/tasks
    *Request.cs                    status, urgent and step requests
src/Modules/Crm/Crm.Infrastructure/Crm.Infrastructure/Adapters/Identity/TaskStaffReader.cs   Identity.Contracts adapter
```

[API contract](../../contracts/crm/tasks.openapi.yaml), [database ownership](../../architecture/data-model/crm.md).

## Tests

- `dotnet test tests/UnitTests/UnitTests.csproj --filter "FullyQualifiedName~UnitTests.Crm.Tasks"` — lanes, statuses, validators, contract.
- `dotnet test tests/IntegrationTests/IntegrationTests.csproj --filter ManagerTaskPersistenceTests` — handlers over PostgreSQL (`BREVIERP_CRM_TEST_CONNECTION`).
