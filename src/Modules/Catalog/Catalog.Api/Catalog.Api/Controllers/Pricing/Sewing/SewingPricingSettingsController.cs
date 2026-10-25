using System.Security.Claims;
using Catalog.Api.Contracts.Pricing.Sewing.Settings;
using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Settings.CreateVersion;
using Catalog.Application.Features.Pricing.Sewing.Settings.GetCurrent;

namespace Catalog.Api.Controllers.Pricing.Sewing;

[ApiController]
[Route("api/v1/pricing/sewing/settings")]
public sealed class SewingPricingSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSewingSettings")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<SewingPricingSettingsSelection>> Get(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSewingSettingsQuery(), cancellationToken));

    [HttpPost("versions", Name = "createSewingSettingsVersion")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<ActionResult<SewingPricingSettingsSelection>> Create(
        [FromBody] CreateSewingSettingsVersionRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor))
            return Forbid();
        if (request.Settings is null)
            return BadRequest();
        var result = await sender.Send(new CreateSewingSettingsVersionCommand(
            request.ExpectedVersionId, request.RequestId, request.Settings.ToSettings(), actor), cancellationToken);
        return this.ToActionResult(result);
    }
}
