using System.Security.Claims;
using Ardalis.Result;
using Catalog.Api.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.List;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Settings;

namespace Catalog.Api.Controllers.Pricing.Ppe;

/// <summary>«Довідники» → «Імпорт цін»: PPE supplier price sources, their discounts and file imports (CAT-017).</summary>
[ApiController]
[Route("api/v1/pricing/ppe/supplier-prices")]
public sealed class PpeSupplierPricesController(ISender sender) : ControllerBase
{
    /// <summary>Gzipped uploads from the admin are a few MB; the limit stays under Cloud Run's 32 MiB request cap.</summary>
    private const long MaxRequestBytes = 30 * 1024 * 1024;

    /// <summary>The director also holds Admin, so this covers both.</summary>
    private const string AdminRole = "Admin";

    [HttpGet(Name = "listPpePriceSources")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<PpePriceSourcesResponse>> List(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPpePriceSourcesQuery(), cancellationToken);
        return result.IsSuccess
            ? Ok(new PpePriceSourcesResponse(User.IsInRole(AdminRole), result.Value))
            : this.ToActionResult(result);
    }

    /// <summary>Changes the discount and supplier of a source; administrators and the director only.</summary>
    [HttpPut("{key}", Name = "updatePpePriceSource")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(string key, UpdatePpePriceSourceRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        var result = await sender.Send(new UpdatePpePriceSourceCommand(
            key, request.DiscountPercent, request.SupplierId, User.Identity?.Name ?? actor), cancellationToken);
        return result.Status switch
        {
            ResultStatus.Ok => NoContent(),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => BadRequest(result.ValidationErrors),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    /// <summary>
    /// Reads the supplier's price files (gzip allowed) and returns every product of the supplier with its old and new
    /// price. With <c>dryRun=false</c> it saves the changed prices; the admin sends the same files again to apply.
    /// </summary>
    [HttpPost("{key}/import", Name = "importPpeSupplierPrices")]
    [Authorize(Policy = "CanManageProducts")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<PpePriceImportResult>> Import(
        string key, [FromForm] IFormFileCollection files, [FromQuery] bool dryRun = true,
        CancellationToken cancellationToken = default)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        var actorName = User.Identity?.Name ?? actor;
        var streams = files.Select(file => new SupplierPriceFile(file.FileName, file.OpenReadStream())).ToList();
        try
        {
            return this.ToActionResult(await sender.Send(
                new ImportPpeSupplierFilesCommand(key, streams, dryRun, actor, actorName), cancellationToken));
        }
        finally
        {
            foreach (var file in streams) await file.Content.DisposeAsync();
        }
    }
}
