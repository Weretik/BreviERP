using Content.Application.Features.ProductPages.Queries.GetContentProduct;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.ProductPages.GetContentProduct;

internal static class GetContentProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{productId:int}", HandleAsync).WithName("getContentProduct");

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetContentProductQuery(productId), cancellationToken)).ToHttpResult();
}
