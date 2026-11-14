using Accounting.Application.Features.Categories.Queries.GetFinanceCategories;
using Accounting.Domain.Ledger;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Categories.GetFinanceCategories;

internal static class GetFinanceCategoriesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceCategories");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, QueryEnum<LedgerScope>? scope = null,
        CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetFinanceCategoriesQuery(scope?.Value), cancellationToken)).ToHttpResult();
}
