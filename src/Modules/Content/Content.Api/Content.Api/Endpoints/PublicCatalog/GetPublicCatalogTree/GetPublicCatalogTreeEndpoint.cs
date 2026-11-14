using Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogTree;

internal static class GetPublicCatalogTreeEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("tree", HandleAsync).WithName("getPublicCatalogTree");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicCatalogTreeQuery(), cancellationToken)).ToHttpResult();
}
