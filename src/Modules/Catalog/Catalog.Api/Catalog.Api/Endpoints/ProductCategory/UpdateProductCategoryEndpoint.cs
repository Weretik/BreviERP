using Catalog.Api.ProductCategory;
using Catalog.Application.Features.ProductCategory.Commands.UpdateProductCategory;

namespace Catalog.Api.Endpoints.ProductCategory;

internal static class UpdateProductCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", HandleAsync);

    private static async Task<IResult> HandleAsync(int id, UpdateProductCategoryRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCategoryCommand(
            id,
            new UpdateProductCategoryCommandRequest(
                request.Name,
                request.RuName,
                request.Slug,
                request.ParentId,
                request.SortOrder,
                request.IsActive,
                request.Description));

        var result = await sender.Send(command, cancellationToken);

        return result.ToHttpResult();
    }
}
