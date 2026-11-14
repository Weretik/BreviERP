using Scheduling.Api.Endpoints.Tasks.GetScheduledTaskRuns;
using Scheduling.Api.Endpoints.Tasks.GetScheduledTasks;
using Scheduling.Api.Endpoints.Tasks.RunScheduledTaskNow;
using Scheduling.Api.Endpoints.Tasks.UpdateScheduledTaskSchedule;

namespace Scheduling.Api.Endpoints.Tasks;

/// <summary>«Фонові задачі»: list, history, schedule, «Запустити зараз».</summary>
internal static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapScheduledTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/scheduling/tasks").WithTags("ScheduledTasks")
            .RequireAuthorization(SchedulingAccess.Page);
        GetScheduledTasksEndpoint.Map(group);
        GetScheduledTaskRunsEndpoint.Map(group);
        UpdateScheduledTaskScheduleEndpoint.Map(group);
        RunScheduledTaskNowEndpoint.Map(group);
        return app;
    }
}
