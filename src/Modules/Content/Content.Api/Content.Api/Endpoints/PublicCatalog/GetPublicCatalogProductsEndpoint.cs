using Content.Application;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogProducts;

namespace Content.Api.Endpoints.PublicCatalog;

internal static class GetPublicCatalogProductsEndpoint
{
    public const int DefaultPageSize = 24;

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("products", HandleAsync).WithName("getPublicCatalogProducts");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, string? type = null, int? sectionId = null, string? category = null,
        int page = 1, int pageSize = DefaultPageSize, CancellationToken cancellationToken = default) =>
        (await sender.Send(
            new GetPublicCatalogProductsQuery(page, pageSize, language, type, sectionId, category),
                cancellationToken)).ToHttpResult();
}
