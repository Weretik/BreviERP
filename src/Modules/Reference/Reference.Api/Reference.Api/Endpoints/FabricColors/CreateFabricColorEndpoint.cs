using Reference.Application.Features.FabricColor.Shared;
using Reference.Application.Features.FabricColor.Commands.CreateFabricColor;

namespace Reference.Api.Endpoints.FabricColors;

internal static class CreateFabricColorEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).WithName("createFabricColor").Produces<int>()
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(FabricColorRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CreateFabricColorCommand(new FabricColorFields(request.Name, request.IsActive,
            request.SortOrder)), cancellationToken)).ToHttpResult();
}

/// <summary>A colour of «Кольори»: a new one or the edited fields.</summary>
public sealed record FabricColorRequest(string Name, bool IsActive = true, int SortOrder = 0);
