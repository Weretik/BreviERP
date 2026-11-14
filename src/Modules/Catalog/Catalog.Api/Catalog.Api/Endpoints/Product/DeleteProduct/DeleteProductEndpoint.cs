using Catalog.Application.Features.Product.Commands.DeleteProduct;

namespace Catalog.Api.Endpoints.Product.DeleteProduct;

internal static class DeleteProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCommand(id), cancellationToken);

        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Results.NoContent()
            : result.ToHttpResult();
    }
}
