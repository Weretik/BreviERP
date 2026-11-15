using Ai.Application.Features.Settings.Queries.GetAiSettings;
using Ai.Application.Features.Settings.Shared;
using BuildingBlocks.Api.Errors;

namespace Ai.Api.Endpoints.Settings.GetAiSettings;

internal static class GetAiSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getAiSettings")
            .WithSummary("Settings, this month's use (UTC) and the editable extra rules")
            .WithDescription("Settings, this month's use (UTC) and the editable extra rules. Defaults before the first save.").Produces<AiSettingsResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetAiSettingsQuery(), cancellationToken)).ToHttpResult();
}
