using Accounting.Api.Access;
using BuildingBlocks.Api.Result;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Accounting.Application.Features.Import.Commands.ImportLegacyFinance;
using Accounting.Application.Features.Import.Shared;

namespace Accounting.Api.Import;

/// <summary>
/// «Імпорт архіву Фінансів»: the finance.json export of the old Calculator, gzipped by the browser so it fits Cloud
/// Run's request limit. <c>dryRun=true</c> only checks and reports; <c>false</c> moves the records in one transaction.
/// </summary>
[ApiController]
[Authorize(Policy = AccountingAccess.Policy)]
public sealed class LegacyFinanceImportController(ISender sender) : ControllerBase
{
    /// <summary>Cloud Run accepts 32 MiB per request; the gzipped export is far smaller.</summary>
    private const long MaxRequestBytes = 32 * 1024 * 1024;

    [HttpPost("api/accounting/legacy-import", Name = "importLegacyFinance")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LegacyFinanceImportReport>> Import([FromForm] ImportLegacyFinanceRequest request,
        [FromQuery] bool dryRun = true, CancellationToken cancellationToken = default)
    {
        if (request.File is null || request.File.Length == 0) return BadRequest(new { error = "Оберіть файл finance.json." });
        await using var content = request.File.OpenReadStream();
        return this.ToActionResult(await sender.Send(new ImportLegacyFinanceCommand(content, dryRun), cancellationToken));
    }
}

/// <param name="File">finance.json of the Calculator export, as is or gzipped.</param>
public sealed record ImportLegacyFinanceRequest(IFormFile File);
