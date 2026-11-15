using Content.Application.Features.Catalog.Queries.GetPublicCatalogTree;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogTree;

internal static class GetPublicCatalogTreeEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("tree", HandleAsync).WithName("getPublicCatalogTree")
            .WithSummary("Get public catalog tree")
            .WithDescription("The two product types and, under each, the active sections of «Типи і розділи» that have at least one product on the site, in the display order of the reference. A type without products is left out.")
            .Produces<PublicCatalogTree>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicCatalogTreeQuery(), cancellationToken)).ToHttpResult();
}
