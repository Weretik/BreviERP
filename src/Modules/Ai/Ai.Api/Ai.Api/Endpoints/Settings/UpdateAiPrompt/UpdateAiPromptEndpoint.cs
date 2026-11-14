using Ai.Application.Features.Settings.Commands.UpdateAiPrompt;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Api.Endpoints.Settings.UpdateAiPrompt;

/// <summary>Extra rules of one request kind, e.g. <c>purchase-invoices</c>; empty text removes them.</summary>
internal static class UpdateAiPromptEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("prompts/{key}", HandleAsync).WithName("updateAiPrompt").Produces<AiSettingsResponse>();

    private static async Task<IResult> HandleAsync(string key, AiPromptRequest request, ClaimsPrincipal user,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateAiPromptCommand(key, request.Text, user.ActorName()), cancellationToken))
        .ToHttpResult();
}

public sealed record AiPromptRequest(string? Text);
