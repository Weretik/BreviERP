using BuildingBlocks.Api.Result;
using Crm.Api.Access;
using Crm.Api.Templates.Requests;
using Crm.Application.Quotes.Introductions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Templates;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/crm/introductions/featured-mapping")]
public sealed class IntroductionFeaturedMappingController(
    IntroductionFeaturedMappingService service, IIntroductionFeaturedMappingStore mappings) : ControllerBase
{
    [HttpGet(Name = "getCrmIntroductionFeaturedMapping")]
    public async Task<ActionResult> Active(CancellationToken cancellationToken)
    {
        if (CrmActorClaims.Read(User) is null) return Forbid();
        var value = await mappings.ReadActiveAsync(cancellationToken);
        return value is null ? NotFound() : Ok(value);
    }

    [HttpPut(Name = "activateCrmIntroductionFeaturedMapping")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Activate(IntroductionFeaturedMappingRequest request,
        CancellationToken cancellationToken)
    {
        var actor = CrmActorClaims.Read(User);
        return actor is null ? Forbid() : this.ToActionResult(await service.ActivateAsync(actor,
            request.SnapshotHash, request.SummerModelCode, request.WinterModelCode,
            cancellationToken));
    }
}
