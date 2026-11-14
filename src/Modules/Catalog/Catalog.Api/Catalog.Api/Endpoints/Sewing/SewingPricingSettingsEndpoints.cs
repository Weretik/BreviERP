using Catalog.Api.Endpoints.Sewing.CreateSewingSettingsVersion;
using Catalog.Api.Endpoints.Sewing.GetSewingSettings;

namespace Catalog.Api.Endpoints.Sewing;

internal static class SewingPricingSettingsEndpoints
{
    public static IEndpointRouteBuilder MapSewingPricingSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/pricing/sewing/settings")
            .WithTags("SewingPricingSettings");
        GetSewingSettingsEndpoint.Map(group);
        CreateSewingSettingsVersionEndpoint.Map(group);
        return app;
    }
}
