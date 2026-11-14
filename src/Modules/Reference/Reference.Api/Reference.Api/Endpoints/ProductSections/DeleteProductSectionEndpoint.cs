using Reference.Application.Features.ProductSection.Commands.DeleteProductSection;

namespace Reference.Api.Endpoints.ProductSections;

internal static class DeleteProductSectionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}", HandleAsync)
            .RequireAuthorization("access:product-sections");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new DeleteProductSectionCommand(id), cancellationToken)).ToHttpResult();
}
