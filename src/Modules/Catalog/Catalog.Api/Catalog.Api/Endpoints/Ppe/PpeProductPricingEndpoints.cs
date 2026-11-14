using Catalog.Application.Features.Pricing.Shared;
using Catalog.Api.Endpoints.Ppe.CreatePpeOfferVersion;
using Catalog.Api.Endpoints.Ppe.GetPpeProductPrices;

namespace Catalog.Api.Endpoints.Ppe;

internal static class PpeProductPricingEndpoints
{
    public static IEndpointRouteBuilder MapPpeProductPricingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/pricing/ppe/products")
            .WithTags("PpeProductPricing");
        GetPpeProductPricesEndpoint.Map(group);
        CreatePpeOfferVersionEndpoint.Map(group);
        return app;
    }
}
