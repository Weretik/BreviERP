using Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;

namespace Catalog.Api.Endpoints.Sewing;

/// <summary>Sewing product card: live price of every Article without saving a calculation (CAT-007).</summary>
internal static class SewingArticleSummaryEndpoints
{
    public static IEndpointRouteBuilder MapSewingArticleSummaryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products/{productId:int}/articles/pricing-summary")
            .WithTags("SewingArticleSummary");
        GetSewingArticleSummaryEndpoint.Map(group);
        return app;
    }
}
