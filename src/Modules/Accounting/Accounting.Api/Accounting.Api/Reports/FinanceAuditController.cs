using Accounting.Api.Access;
using Accounting.Domain.Audit;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Accounting.Application.Features.Audit.Queries.GetFinanceAudit;

namespace Accounting.Api.Reports;

/// <summary>«Бухгалтерія → Журнал змін»: changes of all finance records, newest first.</summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/audit")]
public sealed class FinanceAuditController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceAudit")]
    public async Task<ActionResult<FinanceAuditPageResponse>> List([FromQuery] AuditEntity? entity = null,
        [FromQuery] AuditAction? action = null, [FromQuery] int page = 1, CancellationToken cancellationToken = default) =>
        this.ToActionResult(await sender.Send(new GetFinanceAuditQuery(entity, action, page), cancellationToken));
}
