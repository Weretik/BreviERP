using BuildingBlocks.Api.Authorization;
using System.Security.Claims;
using Catalog.Api.Contracts.Pricing.Ppe;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.CreatePpeRuleVersion;
using Catalog.Application.Features.Pricing.Queries.GetPpeRule;
using Catalog.Application.Features.Pricing.Queries.PreviewPpePrices;
namespace Catalog.Api.Controllers.Pricing.Ppe;
[ApiController]
[Route("api/v1/pricing/ppe/rules")]
public sealed class PpePricingRulesController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getPpePricingRule")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<PpeRuleVersion>> Get([FromQuery] string code, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetPpeRuleQuery(code), cancellationToken));

    [HttpPost("versions", Name = "createPpeRuleVersion")]
    [AccessAction(AccessActionAttribute.Edit)]
    [Authorize(Policy = "access:pricing-parameters")]
    public async Task<ActionResult<PpeRuleVersion>> Create([FromBody] CreatePpeRuleVersionRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        return this.ToActionResult(await sender.Send(new CreatePpeRuleVersionCommand(
            request.ExpectedVersionId, request.RequestId, request.Values.ToValues(), actor), cancellationToken));
    }

    /// <summary>Three PPE prices for a supplier price and rule before saving (page «Новий ЗІЗ», CAT-009).</summary>
    [HttpGet("preview", Name = "previewPpePrices")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<PpePriceCalculation>> Preview(
        [FromQuery] string ruleCode, [FromQuery] decimal supplierPrice, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new PreviewPpePricesQuery(ruleCode, supplierPrice), cancellationToken));
}
