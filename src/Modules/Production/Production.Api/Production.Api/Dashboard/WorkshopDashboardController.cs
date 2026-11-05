using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Dashboard;

namespace Production.Api.Dashboard;

/// <summary>«Огляд виробництва» on the admin home (PRD-D50): everyone with production orders.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.OrdersPolicy)]
[Route("api/production/dashboard")]
public sealed class WorkshopDashboardController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getWorkshopDashboard")]
    public async Task<ActionResult<WorkshopDashboardResponse>> Get(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetWorkshopDashboardQuery(actor), cancellationToken))
            : Forbid();
}
