using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;
using Catalog.Application.Features.Product.Shared.Overview;

namespace Catalog.Api.Endpoints.Overview.GetProductOverview;

/// <summary>
/// One page of a «Товари» tab (CAT-013): searched, filtered, sorted and paged on the server, with published prices
/// and fabric variants. section is a section ID or "none"; active=true/false shows only active or inactive rows.
/// </summary>
internal static class GetProductOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("overview", HandleAsync).WithName("getProductOverview")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, string? type = null,
        string? search = null, string? section = null, bool withoutPrice = false, string? sort = null,
        string? direction = null, int page = 1, int pageSize = 25, bool? active = null,
        CancellationToken cancellationToken = default)
    {
        if (!ProductOverviewTypes.TryParse(type, out var productType))
            return Results.BadRequest("type must be Sewing or Ppe.");
        int? sectionId = null;
        var withoutSection = string.Equals(section, "none", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(section) && !withoutSection)
        {
            if (!int.TryParse(section, out var parsedSection))
                return Results.BadRequest("section must be a section ID or none.");
            sectionId = parsedSection;
        }

        if (!Enum.TryParse<ProductOverviewSort>(sort ?? nameof(ProductOverviewSort.Code), true, out var order)
            || !Enum.IsDefined(order) || int.TryParse(sort, out _))
            return Results.BadRequest("sort must be code, name, price or updated.");
        if (direction is not (null or "asc" or "desc"))
            return Results.BadRequest("direction must be asc or desc.");

        return (await sender.Send(new GetProductOverviewPageQuery(productType, search, sectionId,
            withoutSection, withoutPrice, order, direction == "desc", page, pageSize, active),
                cancellationToken)).ToHttpResult();
    }
}
