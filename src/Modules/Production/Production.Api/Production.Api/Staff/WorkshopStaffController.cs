using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Features.Staff.Commands.CreateWorkshopEmployee;
using Production.Application.Features.Staff.Commands.ImportWorkshopStaff;
using Production.Application.Features.Staff.Commands.SetWorkshopEmployeeActive;
using Production.Application.Features.Staff.Commands.UpdateWorkshopEmployee;
using Production.Application.Features.Staff.Queries.GetWorkshopStaff;
using Production.Application.Features.Staff.Shared;

namespace Production.Api.Staff;

/// <summary>«Працівники цеху»: the workers the head of the workshop records work and pay for.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Staff)]
[Route("api/production/staff")]
public sealed class WorkshopStaffController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getWorkshopStaff")]
    public async Task<ActionResult<IReadOnlyList<WorkshopEmployeeResponse>>> List(
        [FromQuery] bool includeArchived, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetWorkshopStaffQuery(includeArchived), cancellationToken));

    [HttpPost(Name = "createWorkshopEmployee")]
    public async Task<ActionResult<WorkshopEmployeeResponse>> Create(WorkshopEmployeeInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new CreateWorkshopEmployeeCommand(input, this.UserId()), cancellationToken));

    [HttpPut("{id:int}", Name = "updateWorkshopEmployee")]
    public async Task<ActionResult<WorkshopEmployeeResponse>> Update(int id, WorkshopEmployeeInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateWorkshopEmployeeCommand(id, input, this.UserId()), cancellationToken));

    /// <summary>Archives a worker who left, or returns them to the list.</summary>
    [HttpPut("{id:int}/active", Name = "setWorkshopEmployeeActive")]
    public async Task<ActionResult<WorkshopEmployeeResponse>> SetActive(int id, SetWorkshopEmployeeActiveRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SetWorkshopEmployeeActiveCommand(id, request.IsActive, this.UserId()), cancellationToken));

    /// <summary>Adds workers from a list; with <c>dryRun</c> only shows what would be added.</summary>
    [HttpPost("import", Name = "importWorkshopStaff")]
    public async Task<ActionResult<WorkshopStaffImportResult>> Import(
        [FromBody] IReadOnlyList<WorkshopEmployeeImportRow> rows, [FromQuery] bool dryRun, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ImportWorkshopStaffCommand(rows, dryRun, this.UserId()), cancellationToken));
}
