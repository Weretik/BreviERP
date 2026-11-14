using Crm.Api.Access;

namespace Crm.Api.Endpoints.Tasks;

/// <summary>
/// Manager tasks (TSK-001): Director and Admin give, change and delete them in «Адміністрування»; a Manager sees
/// only own tasks on «Мої завдання», moves them and ticks steps. `assigneeUserId` is ignored for a Manager.
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
        GetCrmTaskAssigneesEndpoint.Map(group);
        GetCrmTaskEndpoint.Map(group);
        CreateCrmTaskEndpoint.Map(group);
        EditCrmTaskEndpoint.Map(group);
        ChangeCrmTaskStatusEndpoint.Map(group);
        SetCrmTaskUrgentEndpoint.Map(group);
        MarkCrmTaskStepEndpoint.Map(group);
        DeleteCrmTaskEndpoint.Map(group);
        return app;
    }
}
