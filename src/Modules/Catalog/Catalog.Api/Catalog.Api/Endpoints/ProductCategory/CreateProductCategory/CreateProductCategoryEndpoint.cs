using Catalog.Application.Features.ProductCategory.Commands.CreateProductCategory;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.ProductCategory.CreateProductCategory;

internal static class CreateProductCategoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("", HandleAsync);

    private static async Task<IResult> HandleAsync(CreateProductCategoryRequest request, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCategoryCommand(
            new CreateProductCategoryCommandRequest(
                request.Id,
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
