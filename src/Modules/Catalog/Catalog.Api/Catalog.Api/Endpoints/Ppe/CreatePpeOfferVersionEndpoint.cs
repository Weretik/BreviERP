using Catalog.Api.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Commands.CreatePpeOfferVersion;

namespace Catalog.Api.Endpoints.Ppe;

internal static class CreatePpeOfferVersionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("{productId:int}/offer/versions", HandleAsync).WithName("createPpeOfferVersion")
            .RequireAccessAction(AccessActionAttribute.Edit)
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int productId, CreatePpeOfferVersionRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(new CreatePpeOfferVersionCommand(productId,
            request.ExpectedVersionId, request.RequestId, request.Values.ToValues(), actor),
                cancellationToken)).ToHttpResult();
    }
}
