using Reminders.Application.Features.Occurrences.Commands.CloseReminderOccurrence;
using Reminders.Application.Features.Occurrences.Shared;
using BuildingBlocks.Api.Errors;

namespace Reminders.Api.Endpoints.Occurrences.CloseReminderOccurrence;

internal static class CloseReminderOccurrenceEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/close", HandleAsync).WithName("closeReminderOccurrence").Produces<ReminderOccurrenceResponse>()
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new CloseReminderOccurrenceCommand(id, ReminderAdminClaims.Read(user)), cancellationToken)).ToHttpResult();
}
