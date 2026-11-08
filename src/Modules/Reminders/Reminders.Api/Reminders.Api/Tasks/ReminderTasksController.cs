using BuildingBlocks.Api.Result;
using Mediator;
using Reminders.Api.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reminders.Application.Contracts;
using Reminders.Application.Tasks.ArchiveTask;
using Reminders.Application.Tasks.AssignChat;
using Reminders.Application.Tasks.CreateTask;
using Reminders.Application.Tasks.GetTasks;
using Reminders.Application.Tasks.PreviewDueDates;
using Reminders.Application.Tasks.UpdateTask;

namespace Reminders.Api.Tasks;

[ApiController]
[Authorize(Policy = RemindersAccess.Policy)]
[Route("api/reminders/tasks")]
public sealed class ReminderTasksController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getReminderTasks")]
    public async Task<ActionResult<IReadOnlyList<ReminderTaskResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetReminderTasksQuery(), cancellationToken));

    [HttpPost(Name = "createReminderTask")]
    public async Task<ActionResult<ReminderTaskResponse>> Create(ReminderTaskInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new CreateReminderTaskCommand(input), cancellationToken));

    [HttpPut("{id:long}", Name = "updateReminderTask")]
    public async Task<ActionResult<ReminderTaskResponse>> Update(long id, ReminderTaskInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateReminderTaskCommand(id, input), cancellationToken));

    [HttpDelete("{id:long}", Name = "archiveReminderTask")]
    public async Task<ActionResult> Archive(long id, CancellationToken cancellationToken)
    =>
        this.ToNoContentResult(await sender.Send(new ArchiveReminderTaskCommand(id), cancellationToken));

    [HttpPost("preview", Name = "previewReminderDueDates")]
    public async Task<ActionResult<IReadOnlyList<DueDatePreview>>> Preview(PreviewReminderDueDatesRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new PreviewReminderDueDatesQuery(request.Schedule, request.From), cancellationToken));

    [HttpPost("assign-chat", Name = "assignReminderChat")]
    public async Task<ActionResult<int>> AssignChat(AssignReminderChatRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new AssignReminderChatCommand(request.ChatId, request.Organizations), cancellationToken));
}
