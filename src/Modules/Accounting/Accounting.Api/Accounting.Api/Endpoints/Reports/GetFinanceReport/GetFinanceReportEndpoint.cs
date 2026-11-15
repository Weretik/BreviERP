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
        group.MapGet("", HandleAsync).WithName("getFinanceReport")
            .WithSummary("«Звіти» (stage 8): active income and expense of the inclusive period, grouped by the kind")
            .WithDescription("«Звіти» (stage 8): active income and expense of the inclusive period, grouped by the kind. Totals are per currency and never include transfers; the Accounts kind adds transfers in and out of the period, area and account to each row (type, category and counterparty do not filter them). An ExpenseStructure Group row already holds its Subcategory rows. Operations are oldest first, at most 100 rows (limited = more exist, totalRows = all).")
            .Produces<FinanceReportResponse>();

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
