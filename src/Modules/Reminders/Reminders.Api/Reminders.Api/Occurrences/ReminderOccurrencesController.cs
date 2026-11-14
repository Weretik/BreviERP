using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reminders.Api.Access;
using Reminders.Application.Features.Occurrences.Commands.CloseReminderOccurrence;
using Reminders.Application.Features.Occurrences.Commands.ReopenReminderOccurrence;
using Reminders.Application.Features.Occurrences.Queries.GetReminderOccurrences;
using Reminders.Application.Features.Occurrences.Shared;

namespace Reminders.Api.Occurrences;

[ApiController]
[Authorize(Policy = RemindersAccess.Policy)]
[Route("api/reminders/occurrences")]
public sealed class ReminderOccurrencesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getReminderOccurrences")]
    public async Task<ActionResult<ReminderOccurrencePage>> List([FromQuery] string? status, [FromQuery] long? taskId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetReminderOccurrencesQuery(status, taskId, page, pageSize), cancellationToken));

    [HttpPost("{id:long}/close", Name = "closeReminderOccurrence")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<ReminderOccurrenceResponse>> Close(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new CloseReminderOccurrenceCommand(id, ReminderAdminClaims.Read(User)), cancellationToken));

    [HttpPost("{id:long}/reopen", Name = "reopenReminderOccurrence")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult<ReminderOccurrenceResponse>> Reopen(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ReopenReminderOccurrenceCommand(id, ReminderAdminClaims.Read(User)), cancellationToken));
}
