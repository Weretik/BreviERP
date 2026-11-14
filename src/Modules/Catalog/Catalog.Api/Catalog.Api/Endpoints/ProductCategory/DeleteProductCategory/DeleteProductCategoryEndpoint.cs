using Catalog.Application.Features.ProductCategory.Commands.DeleteProductCategory;

namespace Catalog.Api.Endpoints.ProductCategory.DeleteProductCategory;

internal static class DeleteProductCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteProductCategoryCommand(id), cancellationToken);

        return result.ToHttpResult();
    }
}
