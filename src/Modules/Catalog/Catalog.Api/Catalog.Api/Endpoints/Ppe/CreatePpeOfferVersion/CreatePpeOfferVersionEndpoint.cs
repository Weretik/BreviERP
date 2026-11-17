using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
using Catalog.Application.Features.Pricing.Commands.CreatePpeOfferVersion;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.CreatePpeOfferVersion;

internal static class CreatePpeOfferVersionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{productId:int}/offer/versions", HandleAsync).WithName("createPpeOfferVersion")
            .WithSummary("Create PPE offer version")
            .WithDescription("access:products. Append and approve a version using server actor/time. expectedVersionId null only for first enrollment. Request UUID retries are actor/payload-bound.")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:products")
            .Produces<PpeOfferVersion>();

    private static async Task<IResult> HandleAsync(int productId, CreatePpeOfferVersionRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(new CreatePpeOfferVersionCommand(productId,
            request.ExpectedVersionId, request.RequestId, request.Values?.ToValues(), actor),
                cancellationToken)).ToHttpResult();
    }
}
