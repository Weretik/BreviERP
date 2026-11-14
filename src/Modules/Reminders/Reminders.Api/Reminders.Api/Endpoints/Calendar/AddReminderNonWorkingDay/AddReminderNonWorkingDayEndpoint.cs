using Reminders.Application.Features.Calendar.Commands.AddNonWorkingDay;
using Reminders.Application.Features.Calendar.Shared;

namespace Reminders.Api.Endpoints.Calendar.AddReminderNonWorkingDay;

internal static class AddReminderNonWorkingDayEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("addReminderNonWorkingDay").Produces<NonWorkingDayResponse>();

    private static async Task<IResult> HandleAsync(NonWorkingDayInput input, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new AddNonWorkingDayCommand(input), cancellationToken)).ToHttpResult();
}
