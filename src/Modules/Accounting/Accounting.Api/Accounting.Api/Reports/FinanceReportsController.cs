using Accounting.Api.Access;
using Accounting.Api.Results;
using Accounting.Application.Contracts;
using Accounting.Application.Reports;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Reports;

/// <summary>«Бухгалтерія → Звіти»: a report on the screen and the same report as a CSV file.</summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/reports")]
public sealed class FinanceReportsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceReport")]
    public async Task<ActionResult<FinanceReportResponse>> Get(
        [FromQuery] FinanceReportKind kind,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] LedgerScope? scope = null,
        [FromQuery] Guid? accountId = null,
        [FromQuery] CategoryDirection? type = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] CounterpartyKind? counterpartyKind = null,
        [FromQuery] string? counterpartyId = null,
        CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetFinanceReportQuery(
            Filter(kind, from, to, scope, accountId, type, categoryId, counterpartyKind, counterpartyId)), cancellationToken));

    /// <summary>The report as CSV for Excel; the operations report holds up to 50 000 rows.</summary>
    [HttpGet("csv", Name = "getFinanceReportCsv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "text/csv")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Csv(
        [FromQuery] FinanceReportKind kind,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] LedgerScope? scope = null,
        [FromQuery] Guid? accountId = null,
        [FromQuery] CategoryDirection? type = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] CounterpartyKind? counterpartyKind = null,
        [FromQuery] string? counterpartyId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetFinanceReportQuery(
            Filter(kind, from, to, scope, accountId, type, categoryId, counterpartyKind, counterpartyId),
            FinanceReports.CsvRows), cancellationToken);
        if (!result.IsSuccess) return this.ToFileActionResult(result);
        Response.Headers.CacheControl = "private, no-store";
        return File(FinanceReportCsv.Write(result.Value), "text/csv; charset=utf-8", FinanceReportCsv.FileName(result.Value));
    }

    private static FinanceReportFilter Filter(FinanceReportKind kind, DateOnly from, DateOnly to, LedgerScope? scope,
        Guid? accountId, CategoryDirection? type, Guid? categoryId, CounterpartyKind? counterpartyKind,
        string? counterpartyId) =>
        new(kind, from, to, scope, accountId, type, categoryId,
            counterpartyKind is { } k && !string.IsNullOrWhiteSpace(counterpartyId) ? CounterpartyLink.Of(k, counterpartyId) : null);
}
