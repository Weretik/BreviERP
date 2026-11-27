using Crm.Api.Access;
using Crm.Api.Endpoints.Tasks.ChangeCrmTaskStatus;
using Crm.Api.Endpoints.Tasks.CreateCrmTask;
using Crm.Api.Endpoints.Tasks.DeleteCrmTask;
using Crm.Api.Endpoints.Tasks.EditCrmTask;
using Crm.Api.Endpoints.Tasks.GetCrmTask;
using Crm.Api.Endpoints.Tasks.GetCrmTaskAssignees;
using Crm.Api.Endpoints.Tasks.GetCrmTaskBoard;
using Crm.Api.Endpoints.Tasks.GetCrmTaskOverview;
using Crm.Api.Endpoints.Tasks.GetCrmTasks;
using Crm.Api.Endpoints.Tasks.MarkCrmTaskStep;
using Crm.Api.Endpoints.Tasks.SetCrmTaskUrgent;

namespace Crm.Api.Endpoints.Tasks;

/// <summary>
/// Manager tasks (TSK-001): «Мої завдання» (`tasks`) shows own tasks, moves them and ticks steps; «Завдання менеджерам»
/// (`manager-tasks`, ACC-002 R-01) gives, changes, marks urgent and deletes them, and its «Перегляд» shows every manager's
/// tasks on the same reads. `assigneeUserId` is ignored without it.
/// </summary>
internal static class ManagerTasksEndpoints
{
    public static IEndpointRouteBuilder MapManagerTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/tasks").RequireAuthorization(CrmAccess.Tasks)
            .WithTags("ManagerTasks");
        GetCrmTasksEndpoint.Map(group);
        GetCrmTaskBoardEndpoint.Map(group);
        GetCrmTaskOverviewEndpoint.Map(group);
        GetCrmTaskEndpoint.Map(group);
        ChangeCrmTaskStatusEndpoint.Map(group);
        MarkCrmTaskStepEndpoint.Map(group);

        var manage = app.MapGroup("api/crm/tasks").RequireAuthorization(CrmAccess.ManagerTasks)
            .WithTags("ManagerTasks");
        GetCrmTaskAssigneesEndpoint.Map(manage);
        CreateCrmTaskEndpoint.Map(manage);
        EditCrmTaskEndpoint.Map(manage);
        SetCrmTaskUrgentEndpoint.Map(manage);
        DeleteCrmTaskEndpoint.Map(manage);
        return app;
    }
}
