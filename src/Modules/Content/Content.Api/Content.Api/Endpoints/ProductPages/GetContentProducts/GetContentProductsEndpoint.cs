using Content.Application.Features.ProductPages.Queries.GetContentProducts;
using BuildingBlocks.Api.Errors;

namespace Content.Api.Endpoints.ProductPages.GetContentProducts;

internal static class GetContentProductsEndpoint
{
    public const int DefaultPageSize = 50;

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getContentProducts")
            .WithSummary("One page of every Catalog product (sewing first, by code) with the state of its site page")
            .WithDescription("One page of every Catalog product (sewing first, by code) with the state of its site page. Search matches the code, the name or the address, case-insensitive.")
            .Produces<ContentProductPage>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, int page = 1,
        int pageSize = DefaultPageSize, string? search = null, string? type = null, int? sectionId = null,
        bool? onSite = null, int? categoryId = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(
            new GetContentProductsQuery(page, pageSize, search, type, sectionId, onSite, categoryId),
                cancellationToken)).ToHttpResult();
}
