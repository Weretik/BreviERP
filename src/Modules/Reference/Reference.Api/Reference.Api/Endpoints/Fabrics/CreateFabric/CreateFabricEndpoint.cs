using Reference.Application.Features.Fabric.Commands.CreateFabric;

namespace Reference.Api.Endpoints.Fabrics.CreateFabric;

internal static class CreateFabricEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync).Produces<int>()
            .RequireAuthorization("access:materials");

    private static async Task<IResult> HandleAsync(CreateFabricRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new CreateFabricCommand(new CreateFabricCommandRequest(request.Id, request.Name,
            request.Price, request.ProviderName,
            request.Code, request.NameRu, request.Composition, request.CompositionRu, request.Density, request.IsActive,
            request.Comment, request.ProposalDescription)), cancellationToken)).ToHttpResult();
}

public sealed record CreateFabricRequest(
    int Id,
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
