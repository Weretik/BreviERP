using Content.Application.Features.ProductPages.Queries.GetContentProducts;

namespace Content.Api.Endpoints.ProductPages.GetContentProducts;

internal static class GetContentProductsEndpoint
{
    public const int DefaultPageSize = 50;

    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getContentProducts");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, int page = 1,
        int pageSize = DefaultPageSize, string? search = null, string? type = null, int? sectionId = null,
        bool? onSite = null, int? categoryId = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(
            new GetContentProductsQuery(page, pageSize, search, type, sectionId, onSite, categoryId),
                cancellationToken)).ToHttpResult();
}
