using Reference.Application.Features.ProductSection.Queries.GetProductSections;

namespace Reference.Api.Endpoints.ProductSections.GetProductSections;

internal static class GetProductSectionsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).Produces<List<ProductSectionRowDTO>>()
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetProductSectionsQuery(), cancellationToken)).ToHttpResult();
}
