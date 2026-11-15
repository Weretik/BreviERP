using Ai.Application.Features.Settings.Commands.SetAiKey;
using Ai.Application.Features.Settings.Shared;
using BuildingBlocks.Api.Errors;

namespace Ai.Api.Endpoints.Settings.SetAiKey;

/// <summary>Replaces the key. The response never contains it, only its last four characters.</summary>
internal static class SetAiKeyEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("key", HandleAsync).WithName("setAiKey")
            .WithSummary("Replaces the OpenAI key")
            .WithDescription("Replaces the OpenAI key. It is stored encrypted (ASP.NET Data Protection) and never returned or logged.").Produces<AiSettingsResponse>();

    private static async Task<IResult> HandleAsync(AiKeyRequest request, ClaimsPrincipal user, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new SetAiKeyCommand(request.ApiKey ?? string.Empty, user.ActorName()), cancellationToken))
        .ToHttpResult();
}

/// <summary>Write-only.</summary>
public sealed record AiKeyRequest(string? ApiKey);
