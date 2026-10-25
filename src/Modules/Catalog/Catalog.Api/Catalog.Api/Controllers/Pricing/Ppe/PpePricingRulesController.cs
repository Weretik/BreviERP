using System.Security.Claims;
using Catalog.Api.Contracts.Pricing.Ppe;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Rules.CreateVersion;
using Catalog.Application.Features.Pricing.Ppe.Rules.GetCurrent;
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
    [Authorize(Policy = "CanManageProducts")]
    public async Task<ActionResult<PpeRuleVersion>> Create([FromBody] CreatePpeRuleVersionRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        return this.ToActionResult(await sender.Send(new CreatePpeRuleVersionCommand(
            request.ExpectedVersionId, request.RequestId, request.Values.ToValues(), actor), cancellationToken));
    }
}
