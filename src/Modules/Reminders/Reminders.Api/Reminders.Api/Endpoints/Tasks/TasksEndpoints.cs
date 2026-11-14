namespace Reminders.Api.Endpoints.Tasks;

/// <summary>Recurring deadline tasks: list, create, edit, archive, preview due dates, assign a chat.</summary>
internal static class TasksEndpoints
{
    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reminders/tasks").RequireAuthorization(RemindersAccess.Policy).WithTags("ReminderTasks");
        GetReminderTasksEndpoint.Map(group);
        CreateReminderTaskEndpoint.Map(group);
        UpdateReminderTaskEndpoint.Map(group);
        ArchiveReminderTaskEndpoint.Map(group);
        PreviewReminderDueDatesEndpoint.Map(group);
        AssignReminderChatEndpoint.Map(group);
        return app;
    }
}
