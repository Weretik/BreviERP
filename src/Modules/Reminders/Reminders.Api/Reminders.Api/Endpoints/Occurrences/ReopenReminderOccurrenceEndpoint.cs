using Reminders.Application.Features.Occurrences.Commands.ReopenReminderOccurrence;
using Reminders.Application.Features.Occurrences.Shared;

namespace Reminders.Api.Endpoints.Occurrences;

internal static class ReopenReminderOccurrenceEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{id:long}/reopen", HandleAsync).WithName("reopenReminderOccurrence").Produces<ReminderOccurrenceResponse>()
            .RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(long id, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ReopenReminderOccurrenceCommand(id, ReminderAdminClaims.Read(user)), cancellationToken)).ToHttpResult();
}
