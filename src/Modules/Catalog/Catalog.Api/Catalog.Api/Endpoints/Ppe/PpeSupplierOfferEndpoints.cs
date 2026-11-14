using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;

namespace Catalog.Api.Endpoints.Ppe;

/// <summary>The PPE product card: supplier fields and live three prices (CAT-008).</summary>
internal static class PpeSupplierOfferEndpoints
{
    public static IEndpointRouteBuilder MapPpeSupplierOfferEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products/{productId:int}/ppe-offer")
            .WithTags("PpeSupplierOffer");
        GetPpeSupplierOfferEndpoint.Map(group);
        UpdatePpeSupplierOfferEndpoint.Map(group);
        return app;
    }
}
