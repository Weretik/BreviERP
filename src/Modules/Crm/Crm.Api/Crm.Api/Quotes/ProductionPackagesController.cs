using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Quotes.Requests;
using Crm.Application.CustomSewing.Production.Export;
using Crm.Application.CustomSewing.Production.Reading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Quotes;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/quotes/{quoteId:guid}/production-packages")]
public sealed class ProductionPackagesController(ProductionPackageExporter exporter, ProductionPackageReader reader) : ControllerBase
{
    [HttpPost(Name = "exportCrmProductionPackage")]
    public async Task<ActionResult> Export(Guid quoteId, ExportProductionPackageRequest request, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await exporter.ExportAsync(
            new ProductionPackageRequest(quoteId, request.VersionId, request.RequestId, request.VariantIds), actor, ct));
    }

    [HttpGet("{packageId:guid}", Name = "getCrmProductionPackage")]
    public async Task<ActionResult> Get(Guid quoteId, Guid packageId, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await reader.GetAsync(quoteId, packageId, actor, ct));
    }

    [HttpGet("{packageId:guid}/content", Name = "downloadCrmProductionPackage")]
    public async Task<ActionResult> Download(Guid quoteId, Guid packageId, CancellationToken ct)
    {
        var actor = CrmActorClaims.Read(User);
        if (actor is null) return Forbid();
        Response.Headers.CacheControl = "no-store, private";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        var result = await reader.ReadAsync(quoteId, packageId, actor, ct);
        if (!result.IsSuccess) return this.ToActionResult(result);
        return File(result.Value.Bytes, "application/zip", $"production-{packageId:D}.zip");
    }
}
