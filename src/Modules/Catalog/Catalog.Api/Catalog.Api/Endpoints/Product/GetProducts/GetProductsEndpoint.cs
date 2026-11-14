
namespace Catalog.Api.Endpoints.Product.GetProducts;

internal static class GetProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync);

    private static async Task<IResult> HandleAsync([AsParameters] GetProductListRequest request,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var mappingResult = GetProductListRequestMapper.Map(request);
        if (mappingResult.Status != Ardalis.Result.ResultStatus.Ok)
            return Results.BadRequest(mappingResult.ValidationErrors);

        var result = await sender.Send(mappingResult.Value, cancellationToken);

        return Results.Ok(ProductListResponseMapper.ToResponse(result));
    }
}
