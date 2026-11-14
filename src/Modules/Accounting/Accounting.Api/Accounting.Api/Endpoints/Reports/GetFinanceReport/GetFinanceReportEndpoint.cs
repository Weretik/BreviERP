using Accounting.Application.Features.Reports.Queries.GetFinanceReport;
using Accounting.Application.Features.Reports.Shared;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Api.Endpoints.Reports.Shared;
using BuildingBlocks.Api.Errors;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Reports.GetFinanceReport;

internal static class GetFinanceReportEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFinanceReport");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        QueryEnum<FinanceReportKind>? kind = null, DateOnly? from = null, DateOnly? to = null,
        QueryEnum<LedgerScope>? scope = null, Guid? accountId = null, QueryEnum<CategoryDirection>? type = null,
        Guid? categoryId = null, QueryEnum<CounterpartyKind>? counterpartyKind = null, string? counterpartyId = null,
        CancellationToken cancellationToken = default)
    {
        var filter = FinanceReportFilters.Of(kind?.Value ?? default, from ?? default, to ?? default, scope?.Value,
            accountId, type?.Value, categoryId, counterpartyKind?.Value, counterpartyId);
        return (await sender.Send(new GetFinanceReportQuery(filter), cancellationToken)).ToHttpResult();
    }
}
