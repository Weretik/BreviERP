using BuildingBlocks.Api.Authorization;
using System.Security.Claims;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.CreateSewingSettingsVersion;
using Catalog.Application.Features.Pricing.Queries.GetSewingSettings;
using Catalog.Api.Pricing.Sewing.Settings;

namespace Catalog.Api.Pricing.Sewing;

[ApiController]
[Route("api/v1/pricing/sewing/settings")]
public sealed class SewingPricingSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSewingSettings")]
    [Authorize(Policy = "CatalogRead")]
    public async Task<ActionResult<SewingPricingSettingsSelection>> Get(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetSewingSettingsQuery(), cancellationToken));

    [HttpPost("versions", Name = "createSewingSettingsVersion")]
    [AccessAction(AccessActionAttribute.Edit)]
    [Authorize(Policy = "access:pricing-parameters")]
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
