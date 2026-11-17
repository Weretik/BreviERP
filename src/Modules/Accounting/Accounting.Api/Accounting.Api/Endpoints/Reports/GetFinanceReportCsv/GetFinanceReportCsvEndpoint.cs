using BuildingBlocks.Api.Errors;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport;
using Accounting.Application.Features.Reports.Shared;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Api.Endpoints.Reports.Shared;
using Accounting.Api.Endpoints.Shared;
using Accounting.Application.Features.Reports.Queries.GetFinanceReport.Results;
using BuildingBlocks.Api.Http;

namespace Accounting.Api.Endpoints.Reports.GetFinanceReportCsv;

/// <summary>The report as CSV for Excel; the operations report holds up to 50 000 rows.</summary>
internal static class GetFinanceReportCsvEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("csv", HandleAsync).WithName("getFinanceReportCsv")
            .WithSummary("Get finance report csv")
            .WithDescription("The same report as a download: UTF-8 with BOM, «;» separators, CRLF, every field quoted, sums in hryvnias with a decimal comma, dates dd.MM.yyyy; text starting with = + - @ gets a leading apostrophe. The operations report holds up to 50 000 rows. Cache-Control private, no-store.")
            .Produces(StatusCodes.Status200OK, contentType: "text/csv");

    private static async Task<IResult> HandleAsync(HttpResponse response, [FromServices] ISender sender,
        QueryEnum<FinanceReportKind>? kind = null, DateOnly? from = null, DateOnly? to = null,
        QueryEnum<LedgerScope>? scope = null, Guid? accountId = null, QueryEnum<CategoryDirection>? type = null,
        Guid? categoryId = null, QueryEnum<CounterpartyKind>? counterpartyKind = null, string? counterpartyId = null,
        CancellationToken cancellationToken = default)
    {
        var filter = FinanceReportFilters.Of(kind?.Value ?? default, from ?? default, to ?? default, scope?.Value,
            accountId, type?.Value, categoryId, counterpartyKind?.Value, counterpartyId);
        var result = await sender.Send(new GetFinanceReportQuery(filter, FinanceReports.CsvRows), cancellationToken);
        if (!result.IsSuccess) return result.ToHttpResult();
        response.Headers.CacheControl = "private, no-store";
        return Results.File(FinanceReportCsv.Write(result.Value), "text/csv; charset=utf-8",
            FinanceReportCsv.FileName(result.Value));
    }
}
