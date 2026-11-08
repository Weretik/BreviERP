using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.GetLead;
using Crm.Application.Leads.GetLeadCallResults;
using Crm.Application.Leads.GetLeads;
using Crm.Application.Leads.GetMyDay;
using Crm.Application.Leads.RecordLeadCall;
using Crm.Application.Leads.ReleaseLead;
using Crm.Application.Leads.TakeLead;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Leads;

/// <summary>
/// «Менеджер сервісу › Ліди» (LEAD-001): «Мій день» and «Усі ліди». Everyone with the page sees every lead and takes
/// free ones; only the owner records calls; Director and Admin may give back anybody's lead.
/// </summary>
[ApiController]
[Authorize(Policy = CrmAccess.Leads)]
[Route("api/crm/leads")]
public sealed class LeadsController(ISender sender) : ControllerBase
{
    /// <param name="scope">`All` (default), `Mine` or `Free`.</param>
    /// <param name="stage">`New`, `InWork`, `Talking`, `Offer`, `Client`, `Closed`, or `Open` for every stage in work.</param>
    [HttpGet(Name = "getCrmLeads")]
    [ProducesResponseType<LeadPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadPageResponse>> List([FromQuery] string? search, [FromQuery] string? scope,
        [FromQuery] string? category, [FromQuery] string? stage, [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var actor = LeadActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(
            new GetLeadsQuery(actor, search, scope, category, stage, page, pageSize), cancellationToken));
    }

    [HttpGet("my-day", Name = "getCrmLeadsMyDay")]
    [ProducesResponseType<LeadMyDayResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadMyDayResponse>> MyDay(CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetMyDayQuery(actor), cancellationToken));
    }

    /// <summary>The active call result buttons, in order.</summary>
    [HttpGet("call-results", Name = "getCrmLeadCallResults")]
    [ProducesResponseType<IReadOnlyList<LeadCallResultResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeadCallResultResponse>>> CallResults(
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetLeadCallResultsQuery(true), cancellationToken));

    [HttpGet("{id:long}", Name = "getCrmLead")]
    [ProducesResponseType<LeadDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadDetailResponse>> Detail(long id, CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new GetLeadQuery(actor, id), cancellationToken));
    }

    /// <summary>«Взяти собі»; 409 when somebody else took it first.</summary>
    [HttpPost("{id:long}/take", Name = "takeCrmLead")]
    [AccessAction(AccessActionAttribute.Edit)]
    [ProducesResponseType<LeadDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadDetailResponse>> Take(long id, CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new TakeLeadCommand(actor, id), cancellationToken));
    }

    /// <summary>«Повернути у вільні»: the owner, or Director/Admin; deletes the open «Подзвонити» task.</summary>
    [HttpPost("{id:long}/release", Name = "releaseCrmLead")]
    [AccessAction(AccessActionAttribute.Edit)]
    [ProducesResponseType<LeadDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadDetailResponse>> Release(long id, [FromQuery] int version,
        CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new ReleaseLeadCommand(actor, id, version), cancellationToken));
    }

    /// <summary>Records a call; a next contact makes a «Подзвонити» task in «Мої завдання».</summary>
    [HttpPost("{id:long}/calls", Name = "recordCrmLeadCall")]
    [AccessAction(AccessActionAttribute.Edit)]
    [ProducesResponseType<LeadDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeadDetailResponse>> RecordCall(long id, [FromBody] RecordLeadCallInput input,
        CancellationToken cancellationToken)
    {
        var actor = LeadActorClaims.Read(User);
        if (actor is null) return Forbid();
        return this.ToActionResult(await sender.Send(new RecordLeadCallCommand(actor, id, input), cancellationToken));
    }
}
