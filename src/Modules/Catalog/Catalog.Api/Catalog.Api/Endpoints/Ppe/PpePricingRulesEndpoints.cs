namespace Catalog.Api.Endpoints.Ppe;

internal static class PpePricingRulesEndpoints
{
    public static IEndpointRouteBuilder MapPpePricingRulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/pricing/ppe/rules")
            .WithTags("PpePricingRules");
        GetPpePricingRuleEndpoint.Map(group);
        CreatePpeRuleVersionEndpoint.Map(group);
        PreviewPpePricesEndpoint.Map(group);
        return app;
    }
}
