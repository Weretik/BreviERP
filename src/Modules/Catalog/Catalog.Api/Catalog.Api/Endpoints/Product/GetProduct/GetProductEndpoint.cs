using Catalog.Application.Features.Product.Queries.GetAdminProductDetail;
using Catalog.Api.Endpoints.Product.Detail;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Product.GetProduct;

internal static class GetProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminProductDetailQuery(id), cancellationToken);

        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Results.Ok(ProductDetailResponseMapper.ToResponse(result.Value))
            : result.ToHttpResult();
    }
}
