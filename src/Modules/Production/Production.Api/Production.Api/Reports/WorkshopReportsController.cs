using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Reports.GetReport;

namespace Production.Api.Reports;

/// <summary>«Звіти цеху» (PRD-D48): Admin only; the director also holds Admin.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Reports)]
[Route("api/production/reports")]
public sealed class WorkshopReportsController(ISender sender) : ControllerBase
{
    /// <summary>The months <c>from</c>..<c>to</c> (<c>YYYY-MM</c>) and, with both, the months to compare with.</summary>
    [HttpGet(Name = "getWorkshopReport")]
    public async Task<ActionResult<WorkshopReportPairResponse>> Get([FromQuery] string from, [FromQuery] string to,
        [FromQuery] string? compareFrom, [FromQuery] string? compareTo, CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(
                new GetWorkshopReportQuery(actor, from, to, compareFrom, compareTo), cancellationToken))
            : Forbid();
}
