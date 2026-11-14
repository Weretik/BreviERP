using Accounting.Api.Access;
using Accounting.Domain.Ledger;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Accounting.Application.Features.Overview.Queries.GetFinanceOverview;

namespace Accounting.Api.Overview;

/// <summary>«Бухгалтерія → Огляд». Without <c>scope</c> both areas are counted together.</summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/overview")]
public sealed class FinanceOverviewController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceOverview")]
    public async Task<ActionResult<FinanceOverviewResponse>> Get([FromQuery] DateOnly from, [FromQuery] DateOnly to,
        [FromQuery] LedgerScope? scope = null, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetFinanceOverviewQuery(scope, from, to), cancellationToken));
}
