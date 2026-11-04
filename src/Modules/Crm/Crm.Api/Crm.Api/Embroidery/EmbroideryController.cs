using BuildingBlocks.Api.Result;
using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Calculate;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.GetForm;
using Crm.Application.Embroidery.GetSettings;
using Crm.Application.Embroidery.SaveSettings;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Crm.Api.Embroidery;

/// <summary>
/// «Ціна вишивки» (EMB-001): Manager, Director and Admin calculate; only Director and Admin (the Director also holds
/// Admin) see the cost and change «Налаштування».
/// </summary>
[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/embroidery")]
public sealed class EmbroideryController(ISender sender) : ControllerBase
{
    [HttpGet("form", Name = "getCrmEmbroideryForm")]
    [ProducesResponseType<EmbroideryFormResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EmbroideryFormResponse>> Form(CancellationToken cancellationToken)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetEmbroideryFormQuery(actor), cancellationToken));
    }

    /// <summary>Computes the price without saving anything; a Manager gets no cost.</summary>
    [HttpPost("calculate", Name = "calculateCrmEmbroidery")]
    [ProducesResponseType<EmbroideryCalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmbroideryCalculationResponse>> Calculate(CalculateEmbroideryInput input,
        CancellationToken cancellationToken)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new CalculateEmbroideryQuery(actor, input), cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("settings", Name = "getCrmEmbroiderySettings")]
    [ProducesResponseType<EmbroiderySettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EmbroiderySettingsResponse>> Settings(CancellationToken cancellationToken)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetEmbroiderySettingsQuery(actor), cancellationToken));
    }

    /// <summary>Saves the whole page; 409 when someone saved a newer version.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("settings", Name = "saveCrmEmbroiderySettings")]
    [ProducesResponseType<EmbroiderySettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmbroiderySettingsResponse>> SaveSettings(SaveEmbroiderySettingsInput input,
        CancellationToken cancellationToken)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new SaveEmbroiderySettingsCommand(actor, input), cancellationToken));
    }

    private EmbroideryActor? ReadActor() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? new EmbroideryActor(id, User.IsInRole("Admin"))
            : null;
}
