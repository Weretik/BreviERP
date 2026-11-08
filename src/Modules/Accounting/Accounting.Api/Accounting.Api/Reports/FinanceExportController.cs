using Accounting.Api.Access;
using Accounting.Api.Results;
using Accounting.Application.Export;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Api.Reports;

/// <summary>«Резервна копія»: every finance record in one JSON file, downloaded.</summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
[Route("api/accounting/export")]
public sealed class FinanceExportController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getFinanceExport")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinanceExportQuery(), cancellationToken);
        if (!result.IsSuccess) return this.ToFileActionResult(result);
        Response.Headers.CacheControl = "private, no-store";
        return File(result.Value.Content, "application/json", result.Value.FileName);
    }
}
