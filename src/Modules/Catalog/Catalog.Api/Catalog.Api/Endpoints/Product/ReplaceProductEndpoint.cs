using Catalog.Api.Product.Detail;
using Catalog.Api.Product.Write;
using Catalog.Application.Features.Product.Commands.ReplaceProduct;

namespace Catalog.Api.Endpoints.Product;

internal static class ReplaceProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, ProductWriteRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var mappingResult = ProductWriteRequestMapper.Map(request);
        if (mappingResult.Status != Ardalis.Result.ResultStatus.Ok)
            return Results.BadRequest(mappingResult.ValidationErrors);

        var result = await sender.Send(new ReplaceProductCommand(id, mappingResult.Value), cancellationToken);

        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Results.Ok(ProductDetailResponseMapper.ToResponse(result.Value))
            : result.ToHttpResult();
    }
}
