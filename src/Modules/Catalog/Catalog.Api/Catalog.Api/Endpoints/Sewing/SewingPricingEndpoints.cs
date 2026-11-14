namespace Catalog.Api.Endpoints.Sewing;

internal static class SewingPricingEndpoints
{
    public static IEndpointRouteBuilder MapSewingPricingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products/{productId:int}/articles/{articleId:long}/pricing")
            .WithTags("SewingPricing");
        SaveSewingCalculationEndpoint.Map(group);
        GetSewingCalculationEndpoint.Map(group);
        PublishSewingCalculationEndpoint.Map(group);
        GetPublishedSewingPriceEndpoint.Map(group);
        return app;
    }
}
