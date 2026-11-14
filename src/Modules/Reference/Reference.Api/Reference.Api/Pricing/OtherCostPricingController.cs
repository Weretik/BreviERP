using BuildingBlocks.Api.Authorization;
using Reference.Application.Features.Pricing.Commands.ReviseOtherCost;

namespace Reference.Api.Pricing;

[ApiController]
[Route("api/reference/pricing/other-costs/{code}/versions")]
public sealed class OtherCostPricingController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "reviseOtherCost")]
    [AccessAction(AccessActionAttribute.Edit)]
    [Authorize(Policy = "access:pricing-parameters")]
    public async Task<ActionResult<long>> Revise(
        string code, [FromBody] ReviseOtherCostRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ReviseOtherCostCommand(
            code, request.ExpectedVersionId, request.Value, request.RequestId), cancellationToken));
}
