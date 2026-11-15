using Content.Application.Features.ProductPages.Shared.Responses;
using Content.Application.Features.ProductPages.Queries.GetContentProduct;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.ProductPages.GetContentProduct;

internal static class GetContentProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{productId:int}", HandleAsync).WithName("getContentProduct")
            .WithSummary("The editor of one product")
            .WithDescription("The editor of one product. A product without a page comes with revision 0, empty texts and an offered address built from its name.")
            .Produces<ContentProductResponse>();

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetContentProductQuery(productId), cancellationToken)).ToHttpResult();
}
