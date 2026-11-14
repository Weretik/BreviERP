using Catalog.Application.Features.ProductCategory.Queries.GetAdminProductCategories;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.ProductCategory.GetAdminProductCategories;

internal static class GetAdminProductCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("admin", HandleAsync);

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminProductCategoriesQuery(), cancellationToken);

        return result.ToHttpResult();
    }
}
