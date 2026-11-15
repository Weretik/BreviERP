using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;
using Catalog.Application.Features.Pricing.Commands.UpdatePpeSupplierOffer;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.UpdatePpeSupplierOffer;

internal static class UpdatePpeSupplierOfferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("", HandleAsync).WithName("updatePpeSupplierOffer")
            .WithSummary("Update PPE supplier offer")
            .WithDescription("access:products. Replaces the supplier fields. When the supplier price or the rule changes, appends an approved offer version (source «Картка ЗІЗ», server actor/time) that keeps the Insight prices; expectedOfferVersionId must name the current version (null when none).")
            .RequireAuthorization("access:products")
            .Produces<PpeSupplierOffer>();

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
