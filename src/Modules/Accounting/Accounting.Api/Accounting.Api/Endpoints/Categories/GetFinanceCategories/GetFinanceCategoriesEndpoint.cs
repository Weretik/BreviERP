using Accounting.Application.Features.Categories.Shared;
using Accounting.Application.Features.Categories.Queries.GetFinanceCategories;
using Accounting.Domain.Ledger;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Categories.GetFinanceCategories;

internal static class GetFinanceCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceCategories")
            .WithSummary("«Категорії» of one area of the books (all areas without scope), roots and subcategories in one flat list")
            .Produces<IReadOnlyList<FinanceCategoryResponse>>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, QueryEnum<LedgerScope>? scope = null,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceCategoriesQuery(scope?.Value), cancellationToken)).ToHttpResult();
}
