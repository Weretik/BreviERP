using Microsoft.AspNetCore.RateLimiting;
using Catalog.Application.Features.ProductCategory.Queries.GetStoreProductCategories;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.ProductCategory.GetStoreProductCategories;

internal static class GetStoreProductCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("store/{language}", HandleAsync)
            .WithName("getStoreProductCategories")
            .WithSummary("List product categories of a store")
            .Produces<List<StoreProductCategoryRowDTO>>()
            .AllowAnonymous().RequireRateLimiting("PublicRead");

    private static async Task<IResult> HandleAsync(string language, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStoreProductCategoriesQuery(language), cancellationToken);

        return result.ToHttpResult();
    }
}
