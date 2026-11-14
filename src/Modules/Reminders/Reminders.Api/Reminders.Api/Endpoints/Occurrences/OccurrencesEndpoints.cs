using Reminders.Api.Endpoints.Occurrences.CloseReminderOccurrence;
using Reminders.Api.Endpoints.Occurrences.GetReminderOccurrences;
using Reminders.Api.Endpoints.Occurrences.ReopenReminderOccurrence;

namespace Reminders.Api.Endpoints.Occurrences;

/// <summary>Periods of reminder tasks: list, close, reopen.</summary>
internal static class OccurrencesEndpoints
{
    public static IEndpointRouteBuilder MapOccurrencesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reminders/occurrences").RequireAuthorization(RemindersAccess.Policy).WithTags("ReminderOccurrences");
        GetReminderOccurrencesEndpoint.Map(group);
        CloseReminderOccurrenceEndpoint.Map(group);
        ReopenReminderOccurrenceEndpoint.Map(group);
        return app;
    }
}
