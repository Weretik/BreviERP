using Ai.Application.Features.Settings.Commands.TestAiConnection;
using Ai.Application.Features.Settings.Shared;
using Ai.Domain.Settings;
using BuildingBlocks.Api.Errors;

namespace Ai.Api.Endpoints.Settings.TestAiConnection;

/// <summary>«Перевірити»: 422 when OpenAI refused (key, model, balance), 503 when there is no key or OpenAI is down.</summary>
internal static class TestAiConnectionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("test", HandleAsync).WithName("testAiConnection").Produces<AiTestResponse>();

    private static async Task<IResult> HandleAsync(AiTestRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new TestAiConnectionCommand(request.Tier), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Unavailable => Results.Json(new { errors = result.Errors },
                statusCode: StatusCodes.Status503ServiceUnavailable),
            ResultStatus.Error => Results.UnprocessableEntity(new { errors = result.Errors }),
            _ => result.ToHttpResult(),
        };
    }
}

public sealed record AiTestRequest(AiModelTier Tier);
