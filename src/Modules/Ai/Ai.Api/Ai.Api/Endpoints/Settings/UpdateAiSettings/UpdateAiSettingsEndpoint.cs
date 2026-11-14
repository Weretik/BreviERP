using Ai.Application.Features.Settings.Commands.UpdateAiSettings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Api.Endpoints.Settings.UpdateAiSettings;

/// <summary>409 when the version is stale; 400 when switching on without a key.</summary>
internal static class UpdateAiSettingsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("updateAiSettings").Produces<AiSettingsResponse>();

    private static async Task<IResult> HandleAsync(AiSettingsRequest request, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateAiSettingsCommand(request.Enabled, request.FastModel, request.FastInputPrice,
            request.FastOutputPrice, request.StrongModel, request.StrongInputPrice, request.StrongOutputPrice,
            request.MonthlyLimitUsd, request.Version, user.ActorName()), cancellationToken)).ToHttpResult();
}

public sealed record AiSettingsRequest(
    bool Enabled,
    string FastModel,
    decimal FastInputPrice,
    decimal FastOutputPrice,
    string StrongModel,
    decimal StrongInputPrice,
    decimal StrongOutputPrice,
    decimal MonthlyLimitUsd,
    int Version);
