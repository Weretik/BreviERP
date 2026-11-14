using Ai.Application.Features.Settings.Commands.SetAiKey;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Api.Endpoints.Settings;

/// <summary>Replaces the key. The response never contains it, only its last four characters.</summary>
internal static class SetAiKeyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("key", HandleAsync).WithName("setAiKey").Produces<AiSettingsResponse>();

    private static async Task<IResult> HandleAsync(AiKeyRequest request, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SetAiKeyCommand(request.ApiKey ?? string.Empty, user.ActorName()), cancellationToken))
        .ToHttpResult();
}

/// <summary>Write-only.</summary>
public sealed record AiKeyRequest(string? ApiKey);
