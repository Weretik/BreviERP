namespace Reference.Api.Endpoints.OtherCostPricing;

/// <summary>Versions of the non-inventory service costs used by Sewing pricing.</summary>
internal static class OtherCostPricingEndpoints
{
    public static IEndpointRouteBuilder MapOtherCostPricingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/pricing/other-costs/{code}/versions").WithTags("OtherCostPricing");
        ReviseOtherCostEndpoint.Map(group);
        return app;
    }
}
