using Content.Application.Features.Catalog.Queries.GetPublicCatalogCategories;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.PublicCatalog.GetPublicCatalogCategories;

internal static class GetPublicCatalogCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("categories", HandleAsync).WithName("getPublicCatalogCategories")
            .WithSummary("The category tree of the site")
            .WithDescription("The category tree of the site. Categories marked «На сайті» in their order, each with its marked subcategories. A subcategory of a hidden category is left out. A marked category without products comes back too.")
            .Produces<PublicCatalogCategoryTree>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublicCatalogCategoriesQuery(), cancellationToken)).ToHttpResult();
}
