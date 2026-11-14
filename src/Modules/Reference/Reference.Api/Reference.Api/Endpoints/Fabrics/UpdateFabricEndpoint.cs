using Reference.Application.Features.Fabric.Commands.UpdateFabric;

namespace Reference.Api.Endpoints.Fabrics;

internal static class UpdateFabricEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync)
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(int id, UpdateFabricRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateFabricCommand(id, new UpdateFabricCommandRequest(request.Name, request.Price,
            request.ProviderName, request.Code, request.NameRu, request.Composition, request.CompositionRu,
                request.Density,
            request.IsActive, request.Comment, request.ProposalDescription)), cancellationToken)).ToHttpResult();
}

public sealed record UpdateFabricRequest(
    string Name,
    decimal Price,
    string ProviderName,
    string? Code = null,
    string? NameRu = null,
    string? Composition = null,
    string? CompositionRu = null,
    string? Density = null,
    bool IsActive = true,
    string? Comment = null,
    string? ProposalDescription = null);
