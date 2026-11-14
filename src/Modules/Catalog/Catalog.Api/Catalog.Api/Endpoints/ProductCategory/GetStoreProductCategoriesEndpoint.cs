using Catalog.Application.Features.ProductCategory.Queries.GetStoreProductCategories;

namespace Catalog.Api.Endpoints.ProductCategory;

internal static class GetStoreProductCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("store/{language}", HandleAsync)
            .AllowAnonymous();

    private static async Task<IResult> HandleAsync(string language, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreProductCategoriesQuery(language), cancellationToken);

        return result.ToHttpResult();
    }
}
