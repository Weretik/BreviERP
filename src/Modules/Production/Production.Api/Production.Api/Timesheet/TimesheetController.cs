using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Timesheet;

namespace Production.Api.Timesheet;

/// <summary>«Робочий час»: the head of the workshop and Admin mark the hours of each worker (PRD-D41).</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Policy)]
[Route("api/production/timesheet")]
public sealed class TimesheetController(ISender sender) : ControllerBase
{
    /// <summary>A day with the totals of its month.</summary>
    [HttpGet(Name = "getTimesheetDay")]
    public async Task<ActionResult<TimesheetDayResponse>> Day([FromQuery] DateOnly date,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetTimesheetDayQuery(actor, date), cancellationToken))
            : Forbid();

    /// <summary>Saves the changed marks of a day.</summary>
    [HttpPut(Name = "saveTimesheetDay")]
    public async Task<ActionResult<TimesheetDayResponse>> Save(SaveTimesheetDayInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new SaveTimesheetDayCommand(actor, input), cancellationToken))
            : Forbid();
}
