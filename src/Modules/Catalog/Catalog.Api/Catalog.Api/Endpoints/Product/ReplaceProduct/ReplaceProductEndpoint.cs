using Catalog.Application.Features.Product.Commands.ReplaceProduct;
using Catalog.Api.Endpoints.Product.Detail;
using Catalog.Api.Endpoints.Product.Write;

namespace Catalog.Api.Endpoints.Product.ReplaceProduct;

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
