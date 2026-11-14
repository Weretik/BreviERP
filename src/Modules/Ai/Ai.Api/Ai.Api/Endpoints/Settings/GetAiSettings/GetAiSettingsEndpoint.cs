using Ai.Application.Features.Settings.Queries.GetAiSettings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Api.Endpoints.Settings.GetAiSettings;

internal static class GetAiSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getAiSettings").Produces<AiSettingsResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetAiSettingsQuery(), cancellationToken)).ToHttpResult();
}
