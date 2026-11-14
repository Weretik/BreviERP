using Content.Application;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProduct;

namespace Content.Api.Endpoints.PublicCatalog;

internal static class GetPublicCatalogProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("products/{slug}", HandleAsync).WithName("getPublicCatalogProduct");

    private static async Task<IResult> HandleAsync(string slug, [FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicCatalogProductQuery(slug, language), cancellationToken)).ToHttpResult();
}
