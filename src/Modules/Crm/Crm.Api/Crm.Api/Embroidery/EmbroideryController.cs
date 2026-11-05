using BuildingBlocks.Api.Result;
using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Calculate;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.DeleteCalculation;
using Crm.Application.Embroidery.GetCalculation;
using Crm.Application.Embroidery.GetCalculations;
using Crm.Application.Embroidery.GetForm;
using Crm.Application.Embroidery.GetSettings;
using Crm.Application.Embroidery.ImportArchive;
using Crm.Application.Embroidery.SaveCalculation;
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

    /// <summary>«Збережені»: newest first; search by name, client or «ВШ-0042».</summary>
    [HttpGet("calculations", Name = "getCrmEmbroideryCalculations")]
    [ProducesResponseType<EmbroiderySavedPageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmbroiderySavedPageResponse>> Calculations([FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetEmbroideryCalculationsQuery(actor, search, page, pageSize),
            cancellationToken));
    }

    /// <summary>One saved calculation as it was saved; a Manager gets no cost and no rates.</summary>
    [HttpGet("calculations/{id:long}", Name = "getCrmEmbroideryCalculation")]
    [ProducesResponseType<EmbroiderySavedCalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmbroiderySavedCalculationResponse>> Calculation(long id,
        CancellationToken cancellationToken)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetEmbroideryCalculationQuery(actor, id), cancellationToken));
    }

    /// <summary>«Зберегти розрахунок»: the server prices the form with the current rates and freezes them.</summary>
    [HttpPost("calculations", Name = "saveCrmEmbroideryCalculation")]
    [ProducesResponseType<EmbroiderySavedCalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmbroiderySavedCalculationResponse>> SaveCalculation(
        SaveEmbroideryCalculationInput input, CancellationToken cancellationToken)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new SaveEmbroideryCalculationCommand(actor, input),
            cancellationToken));
    }

    /// <summary>
    /// «Імпорт архіву» (Director and Admin): moves the old calculator's saved calculations; with <c>dryRun</c> only
    /// shows what would happen.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("calculations/import", Name = "importCrmEmbroideryArchive")]
    [ProducesResponseType<EmbroideryArchiveImportResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<EmbroideryArchiveImportResult>> ImportArchive(
        [FromBody] IReadOnlyList<EmbroideryArchiveRow> rows, [FromQuery] bool dryRun,
        CancellationToken cancellationToken)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new ImportEmbroideryArchiveCommand(actor, rows, dryRun),
            cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("calculations/{id:long}", Name = "deleteCrmEmbroideryCalculation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCalculation(long id, CancellationToken cancellationToken)
    {
        var actor = ReadActor();
        if (actor is null) return Forbid();
        var result = await sender.Send(new DeleteEmbroideryCalculationCommand(actor, id), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
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
