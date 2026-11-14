using Catalog.Application.Features.Pricing.Commands.UpdatePpeSupplierOffer;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.UpdatePpeSupplierOffer;

internal static class UpdatePpeSupplierOfferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("updatePpeSupplierOffer")
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int productId, UpdatePpeSupplierOfferRequest request,
        ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var actor = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Results.Forbid();
        return (await sender.Send(new UpdatePpeSupplierOfferCommand(productId, request.SupplierId,
            request.SupplierSku, request.SupplierUrl, request.SupplierPrice, request.AllowManualPrice, request.RuleCode,
            request.Unit, request.ExpectedOfferVersionId, actor), cancellationToken)).ToHttpResult();
    }
}
