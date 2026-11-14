using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Mediator;
using Reminders.Api.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reminders.Application.Features.Chats.Commands.SendReminderChatTest;
using Reminders.Application.Features.Chats.Commands.UpdateReminderChat;
using Reminders.Application.Features.Chats.Queries.GetReminderChats;
using Reminders.Application.Features.Chats.Shared;

namespace Reminders.Api.Chats;

[ApiController]
[Authorize(Policy = RemindersAccess.Policy)]
[Route("api/reminders/chats")]
public sealed class ReminderChatsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getReminderChats")]
    public async Task<ActionResult<IReadOnlyList<ReminderChatResponse>>> List(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetReminderChatsQuery(), cancellationToken));

    [HttpPut("{id:long}", Name = "updateReminderChat")]
    public async Task<ActionResult<ReminderChatResponse>> Update(long id, ReminderChatInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateReminderChatCommand(id, input), cancellationToken));

    [HttpPost("{id:long}/test-message", Name = "sendReminderChatTest")]
    [AccessAction(AccessActionAttribute.Edit)]
    public async Task<ActionResult> SendTest(long id, CancellationToken cancellationToken)
    =>
        this.ToNoContentResult(await sender.Send(new SendReminderChatTestCommand(id), cancellationToken));
}
