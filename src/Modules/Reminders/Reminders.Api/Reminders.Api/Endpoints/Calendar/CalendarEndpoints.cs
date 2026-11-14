using Reminders.Api.Endpoints.Calendar.AddReminderNonWorkingDay;
using Reminders.Api.Endpoints.Calendar.GetReminderNonWorkingDays;
using Reminders.Api.Endpoints.Calendar.RemoveReminderNonWorkingDay;

namespace Reminders.Api.Endpoints.Calendar;

/// <summary>Non-working days of the reminder calendar.</summary>
internal static class CalendarEndpoints
{
    public static IEndpointRouteBuilder MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reminders/non-working-days").RequireAuthorization(RemindersAccess.Policy).WithTags("NonWorkingDays");
        GetReminderNonWorkingDaysEndpoint.Map(group);
        AddReminderNonWorkingDayEndpoint.Map(group);
        RemoveReminderNonWorkingDayEndpoint.Map(group);
        return app;
    }
}
