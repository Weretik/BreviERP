using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.Quotes.Settings.Contracts;
using Crm.Application.Quotes.Settings.GetSettings;
using Crm.Application.Quotes.Settings.SaveSettings;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

/// <summary>«Налаштування КП» (R-13): Director and Admin only (the Director also holds Admin).</summary>
[ApiController]
[Authorize(Policy = CrmAccess.QuoteSettings)]
[Route("api/crm/quote-settings")]
public sealed class QuoteSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getCrmQuoteSettings")]
    [ProducesResponseType<QuoteSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<QuoteSettingsResponse>> Get(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetQuoteSettingsQuery(), cancellationToken));

    /// <summary>Saves the whole page; 409 when someone saved a newer version.</summary>
    [HttpPut(Name = "saveCrmQuoteSettings")]
    [ProducesResponseType<QuoteSettingsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteSettingsResponse>> Save(SaveQuoteSettingsInput input,
        CancellationToken cancellationToken)
    {
        var actor = QuoteActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new SaveQuoteSettingsCommand(actor, input), cancellationToken));
    }
}
