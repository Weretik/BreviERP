using BuildingBlocks.Api.Authorization;
using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Application.Leads.Contracts;
using Crm.Application.Leads.GetLeadCallResults;
using Crm.Application.Leads.References;
using Crm.Application.Leads.Stats;
using Crm.Application.Leads.Sync;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Leads;

/// <summary>
/// «Адміністрування › Ліди» (LEAD-001): «Зведення» (the old «Сводка»), «Довідники» (the old «Справочники») and
/// «Синхронізувати зараз». By default Director and Admin.
/// </summary>
[ApiController]
[Authorize(Policy = CrmAccess.LeadStats)]
[Route("api/crm/lead-stats")]
public sealed class LeadStatsController(ISender sender) : ControllerBase
{
    /// <param name="from">Kyiv date, included; default 6 days before `to`.</param>
    /// <param name="to">Kyiv date, included; default today.</param>
    [HttpGet(Name = "getCrmLeadStats")]
    [ProducesResponseType<LeadStatsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LeadStatsResponse>> Stats([FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetLeadStatsQuery(from, to), cancellationToken));

    /// <summary>All call results, switched-off ones included.</summary>
    [HttpGet("call-results", Name = "getCrmLeadStatsCallResults")]
    [ProducesResponseType<IReadOnlyList<LeadCallResultResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeadCallResultResponse>>> CallResults(
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetLeadCallResultsQuery(false), cancellationToken));

    /// <summary>Adding a result is a change of the references, so it needs edit (not create, which is the sync).</summary>
    [HttpPost("call-results", Name = "createCrmLeadCallResult")]
    [AccessAction(AccessActionAttribute.Edit)]
    [ProducesResponseType<LeadCallResultResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeadCallResultResponse>> CreateCallResult([FromBody] SaveLeadCallResultInput input,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SaveLeadCallResultCommand(null, input), cancellationToken));

    [HttpPut("call-results/{id:long}", Name = "editCrmLeadCallResult")]
    [ProducesResponseType<LeadCallResultResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadCallResultResponse>> EditCallResult(long id,
        [FromBody] SaveLeadCallResultInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SaveLeadCallResultCommand(id, input), cancellationToken));

    [HttpGet("categories", Name = "getCrmLeadCategories")]
    [ProducesResponseType<IReadOnlyList<LeadCategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeadCategoryResponse>>> Categories(
        CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new GetLeadCategoriesQuery(), cancellationToken));

    [HttpPut("categories/{code}", Name = "editCrmLeadCategory")]
    [ProducesResponseType<LeadCategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeadCategoryResponse>> EditCategory(string code,
        [FromBody] SaveLeadCategoryInput input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SaveLeadCategoryCommand(code, input), cancellationToken));

    /// <summary>«Синхронізувати зараз»: reads the sheet now; 503 when Google cannot be read, 409 when a sync runs.</summary>
    [HttpPost("sync", Name = "syncCrmLeads")]
    [Authorize(Policy = CrmAccess.LeadSync)]
    [ProducesResponseType<LeadSyncResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<LeadSyncResponse>> Sync(CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new SyncLeadsCommand(), cancellationToken));
}
