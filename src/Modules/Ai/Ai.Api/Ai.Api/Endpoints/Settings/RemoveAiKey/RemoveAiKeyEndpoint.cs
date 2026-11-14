using Ai.Application.Features.Settings.Commands.RemoveAiKey;
using Ai.Application.Features.Settings.Shared;
using BuildingBlocks.Api.Errors;

namespace Ai.Api.Endpoints.Settings.RemoveAiKey;

/// <summary>Removes the key and switches the AI off.</summary>
internal static class RemoveAiKeyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("key", HandleAsync).WithName("removeAiKey").Produces<AiSettingsResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new RemoveAiKeyCommand(user.ActorName()), cancellationToken)).ToHttpResult();
}
