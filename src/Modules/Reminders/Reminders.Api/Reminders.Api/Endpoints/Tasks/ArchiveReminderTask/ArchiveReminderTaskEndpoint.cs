using Reminders.Application.Features.Tasks.Commands.ArchiveReminderTask;
using BuildingBlocks.Api.Errors;

namespace Reminders.Api.Endpoints.Tasks.ArchiveReminderTask;

internal static class ArchiveReminderTaskEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:long}", HandleAsync).WithName("archiveReminderTask");

    private static async Task<IResult> HandleAsync(long id, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new ArchiveReminderTaskCommand(id), cancellationToken)).ToNoContentHttpResult();
}
