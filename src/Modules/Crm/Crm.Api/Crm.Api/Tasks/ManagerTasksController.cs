using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Tasks.Requests;
using Crm.Application.Tasks.ChangeTaskStatus;
using Crm.Application.Tasks.Contracts;
using Crm.Application.Tasks.CreateTask;
using Crm.Application.Tasks.DeleteTask;
using Crm.Application.Tasks.EditTask;
using Crm.Application.Tasks.GetTask;
using Crm.Application.Tasks.GetTaskAssignees;
using Crm.Application.Tasks.GetTaskBoard;
using Crm.Application.Tasks.GetTaskOverview;
using Crm.Application.Tasks.GetTasks;
using Crm.Application.Tasks.MarkTaskStep;
using Crm.Application.Tasks.SetTaskUrgent;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Tasks;

/// <summary>
/// Manager tasks (TSK-001): Director and Admin give, change and delete them in «Адміністрування»; a Manager sees
/// only own tasks on «Мої завдання», moves them and ticks steps. `assigneeUserId` is ignored for a Manager.
/// </summary>
[ApiController]
[Authorize(Policy = CrmAccess.Tasks)]
[Route("api/crm/tasks")]
public sealed class ManagerTasksController(ISender sender) : ControllerBase
{
    /// <param name="filter">`Open` (default), `Overdue`, `Urgent`, `New`, `InProgress` or `Done`.</param>
    [HttpGet(Name = "getCrmTasks")]
    [ProducesResponseType<TaskPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskPageResponse>> List([FromQuery] string? search, [FromQuery] string? filter,
        [FromQuery] int? assigneeUserId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new GetTasksQuery(actor, search, filter, assigneeUserId, page, pageSize), cancellationToken));
    }

    [HttpGet("board", Name = "getCrmTaskBoard")]
    [ProducesResponseType<TaskBoardResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskBoardResponse>> Board([FromQuery] int? assigneeUserId,
        CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetTaskBoardQuery(actor, assigneeUserId), cancellationToken));
    }

    [HttpGet("overview", Name = "getCrmTaskOverview")]
    [ProducesResponseType<TaskOverviewResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskOverviewResponse>> Overview([FromQuery] int? assigneeUserId,
        CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetTaskOverviewQuery(actor, assigneeUserId),
            cancellationToken));
    }

    [HttpGet("assignees", Name = "getCrmTaskAssignees")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IReadOnlyList<TaskAssigneeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskAssigneeResponse>>> Assignees(
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetTaskAssigneesQuery(), cancellationToken));

    [HttpGet("{id:long}", Name = "getCrmTask")]
    [ProducesResponseType<TaskDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDetailResponse>> Detail(long id, CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetTaskQuery(actor, id), cancellationToken));
    }

    [HttpPost(Name = "createCrmTask")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<TaskDetailResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDetailResponse>> Create(SaveTaskInput input, CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new CreateTaskCommand(actor, input), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Detail), new { id = result.Value.Id }, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPut("{id:long}", Name = "editCrmTask")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<TaskDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDetailResponse>> Edit(long id, SaveTaskInput input,
        CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new EditTaskCommand(actor, id, input), cancellationToken));
    }

    [HttpPut("{id:long}/status", Name = "changeCrmTaskStatus")]
    [ProducesResponseType<TaskDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDetailResponse>> ChangeStatus(long id, ChangeTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new ChangeTaskStatusCommand(actor, id, request.Version, request.Status), cancellationToken));
    }

    [HttpPut("{id:long}/urgent", Name = "setCrmTaskUrgent")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<TaskDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDetailResponse>> SetUrgent(long id, SetTaskUrgentRequest request,
        CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new SetTaskUrgentCommand(actor, id, request.Version, request.IsUrgent), cancellationToken));
    }

    [HttpPut("{id:long}/steps/{stepId:long}", Name = "markCrmTaskStep")]
    [ProducesResponseType<TaskDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskDetailResponse>> MarkStep(long id, long stepId, MarkTaskStepRequest request,
        CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new MarkTaskStepCommand(actor, id, stepId, request.Version, request.IsDone), cancellationToken));
    }

    [HttpDelete("{id:long}", Name = "deleteCrmTask")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(long id, [FromQuery] int version, CancellationToken cancellationToken)
    {
        var actor = TaskActorClaims.Read(User);
        if (actor is null) return Forbid();
        var result = await sender.Send(new DeleteTaskCommand(actor, id, version), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }
}
