using Catalog.Application.Features.Product.Shared.Sewing;

namespace Catalog.Api.Endpoints.Sewing;

internal static class SewingCompositionEndpoints
{
    public static IEndpointRouteBuilder MapSewingCompositionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products/{productId:int}")
            .WithTags("SewingComposition");
        GetSewingCompositionEndpoint.Map(group);
        ReplaceSewingCompositionEndpoint.Map(group);
        return app;
    }
}
