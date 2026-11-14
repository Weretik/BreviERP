using Reminders.Application.Features.Calendar.Queries.GetNonWorkingDays;
using Reminders.Application.Features.Calendar.Shared;

namespace Reminders.Api.Endpoints.Calendar;

internal static class GetReminderNonWorkingDaysEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getReminderNonWorkingDays").Produces<IReadOnlyList<NonWorkingDayResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetNonWorkingDaysQuery(), cancellationToken)).ToHttpResult();
}
