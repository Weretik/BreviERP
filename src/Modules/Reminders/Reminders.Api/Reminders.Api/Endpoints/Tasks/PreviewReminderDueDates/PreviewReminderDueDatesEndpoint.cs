using Reminders.Application.Features.Tasks.Queries.PreviewReminderDueDates;
using Reminders.Application.Features.Tasks.Shared;
using BuildingBlocks.Api.Errors;

namespace Reminders.Api.Endpoints.Tasks.PreviewReminderDueDates;

internal static class PreviewReminderDueDatesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("preview", HandleAsync).WithName("previewReminderDueDates").Produces<IReadOnlyList<DueDatePreview>>()
            .RequireAccessAction(AccessActionAttribute.View);

    private static async Task<IResult> HandleAsync(PreviewReminderDueDatesRequest request, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new PreviewReminderDueDatesQuery(request.Schedule, request.From), cancellationToken)).ToHttpResult();
}

public sealed record PreviewReminderDueDatesRequest(ReminderScheduleInput Schedule, DateOnly? From);
