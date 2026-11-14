using Reminders.Application.Features.Calendar.Commands.RemoveNonWorkingDay;

namespace Reminders.Api.Endpoints.Calendar;

internal static class RemoveReminderNonWorkingDayEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{date}", HandleAsync).WithName("removeReminderNonWorkingDay");

    private static async Task<IResult> HandleAsync(DateOnly date, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new RemoveNonWorkingDayCommand(date), cancellationToken)).ToNoContentHttpResult();
}
