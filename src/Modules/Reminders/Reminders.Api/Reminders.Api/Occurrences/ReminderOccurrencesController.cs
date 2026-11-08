using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reminders.Api.Access;
using Reminders.Application.Contracts;
using Reminders.Application.Occurrences.CloseOccurrence;
using Reminders.Application.Occurrences.GetOccurrences;
using Reminders.Application.Occurrences.ReopenOccurrence;

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
    public async Task<ActionResult<ReminderOccurrenceResponse>> Close(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new CloseReminderOccurrenceCommand(id, ReminderAdminClaims.Read(User)), cancellationToken));

    [HttpPost("{id:long}/reopen", Name = "reopenReminderOccurrence")]
    public async Task<ActionResult<ReminderOccurrenceResponse>> Reopen(long id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new ReopenReminderOccurrenceCommand(id, ReminderAdminClaims.Read(User)), cancellationToken));
}
