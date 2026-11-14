using Catalog.Application.Features.Product.Commands.CreateProduct;
using Catalog.Api.Endpoints.Product.Detail;
using Catalog.Api.Endpoints.Product.Write;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Product.CreateProduct;

internal static class CreateProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync);

    private static async Task<IResult> HandleAsync(CreateProductRequest request, HttpRequest http,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var mappingResult = ProductWriteRequestMapper.Map(request);
        if (mappingResult.Status != Ardalis.Result.ResultStatus.Ok)
            return Results.BadRequest(mappingResult.ValidationErrors);

        var result = await sender.Send(new CreateProductCommand(mappingResult.Value), cancellationToken);

        if (result.Status != Ardalis.Result.ResultStatus.Ok)
            return result.ToHttpResult();

        var response = ProductDetailResponseMapper.ToResponse(result.Value);
        // An absolute Location, as MVC's CreatedAtAction wrote it: GetById has no route name to link to.
        return Results.Created($"{http.Scheme}://{http.Host}{http.PathBase}/api/v1/products/{response.Id}", response);
    }
}
