using Content.Application;
using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategory;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogCategory;

internal static class GetPublicCatalogCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("categories/{slug}", HandleAsync).WithName("getPublicCatalogCategory")
            .WithSummary("A category page by its address with SEO and text")
            .WithDescription("A category page by its address with SEO and text. A Russian request for a category without Russian texts gets the Ukrainian ones, and language says which came back.")
            .Produces<PublicCatalogCategoryPage>();

    private static async Task<IResult> HandleAsync(string slug, [FromServices] ISender sender,
        string language = SiteLanguages.Ukrainian, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetPublicCatalogCategoryQuery(slug, language), cancellationToken)).ToHttpResult();
}
