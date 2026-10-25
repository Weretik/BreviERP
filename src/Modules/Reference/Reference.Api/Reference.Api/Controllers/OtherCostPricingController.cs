using Reference.Api.Contracts.Pricing;
using Reference.Application.Features.Pricing.ReviseOtherCost;

namespace Reference.Api.Controllers;

[ApiController]
[Route("api/reference/pricing/other-costs/{code}/versions")]
public sealed class OtherCostPricingController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "reviseOtherCost")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<ActionResult<long>> Revise(
        string code, [FromBody] ReviseOtherCostRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ReviseOtherCostCommand(
            code, request.ExpectedVersionId, request.Value, request.RequestId), cancellationToken));
}
