using Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;
using Catalog.Application.Features.Product.Shared.Overview;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Overview.GetProductOverview;

/// <summary>
/// One page of a «Товари» tab (CAT-013): searched, filtered, sorted and paged on the server, with published prices
/// and fabric variants. section is a section ID or "none"; active=true/false shows only active or inactive rows.
/// </summary>
internal static class GetProductOverviewEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("overview", HandleAsync).WithName("getProductOverview")
            .WithSummary("One page of a «Товари» tab (CAT-013)")
            .WithDescription("CatalogRead. The server searches, filters, sorts and pages the rows of one type and returns them with their prices, so the list needs this single request. Prices are read, never recalculated: sewing prices are the stored published prices of each fabric variant (automatic publication keeps them current, CAT-012); PPE prices come from the current offer and rule. prices is the lowest price of each tier among the active variants. problems lists the reasons a price is missing (of the row or of an active variant) and is empty when everything is priced. sections and withoutPrice count every row of the type, for the filter chips.")
            .RequireAuthorization("CatalogRead")
            .Produces<ProductOverviewPage>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, string? type = null,
        string? search = null, string? section = null, bool withoutPrice = false, string? sort = null,
        string? direction = null, int page = 1, int pageSize = 25, bool? active = null,
        CancellationToken cancellationToken = default)
    {
        if (!ProductOverviewTypes.TryParse(type, out var productType))
            return ApiProblems.BadRequest("Тип: Sewing або Ppe.");
        int? sectionId = null;
        var withoutSection = string.Equals(section, "none", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(section) && !withoutSection)
        {
            if (!int.TryParse(section, out var parsedSection))
                return ApiProblems.BadRequest("Розділ: його номер або none.");
            sectionId = parsedSection;
        }

        if (!Enum.TryParse<ProductOverviewSort>(sort ?? nameof(ProductOverviewSort.Code), true, out var order)
            || !Enum.IsDefined(order) || int.TryParse(sort, out _))
            return ApiProblems.BadRequest("Сортування: code, name, price або updated.");
        if (direction is not (null or "asc" or "desc"))
            return ApiProblems.BadRequest("Напрям: asc або desc.");

        return (await sender.Send(new GetProductOverviewPageQuery(productType, search, sectionId,
            withoutSection, withoutPrice, order, direction == "desc", page, pageSize, active),
                cancellationToken)).ToHttpResult();
    }
}
