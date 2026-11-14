using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;

namespace Content.Api.Endpoints.PublicCatalog;

internal static class GetPublicCatalogCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("categories", HandleAsync).WithName("getPublicCatalogCategories");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicCatalogCategoriesQuery(), cancellationToken)).ToHttpResult();
}
