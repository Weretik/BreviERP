using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Production.Api.Access;
using Production.Application.Contracts;
using Production.Application.Settings;

namespace Production.Api.Settings;

/// <summary>«Налаштування цеху»: where the bot posts new orders.</summary>
[ApiController]
[Authorize(Policy = ProductionAccess.Settings)]
[Route("api/production/settings")]
public sealed class WorkshopSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getWorkshopSettings")]
    public async Task<ActionResult<WorkshopSettingsResponse>> Get(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new GetWorkshopSettingsQuery(actor), cancellationToken))
            : Forbid();

    [HttpPut(Name = "updateWorkshopSettings")]
    public async Task<ActionResult<WorkshopSettingsResponse>> Update(WorkshopSettingsInput input,
        CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToActionResult(await sender.Send(new UpdateWorkshopSettingsCommand(actor, input), cancellationToken))
            : Forbid();

    /// <summary>Sends a test line to the chosen group and topic.</summary>
    [HttpPost("telegram-test", Name = "sendWorkshopTelegramTest")]
    public async Task<ActionResult> TestTelegram(CancellationToken cancellationToken) =>
        this.Actor() is { } actor
            ? this.ToNoContentResult(await sender.Send(new SendWorkshopTelegramTestCommand(actor), cancellationToken))
            : Forbid();
}
