using Reference.Api.Endpoints.PricingParameters.CreatePricingParameter;
using Reference.Api.Endpoints.PricingParameters.DeletePricingParameter;
using Reference.Api.Endpoints.PricingParameters.GetPricingParameters;
using Reference.Api.Endpoints.PricingParameters.ImportPricingSettings;
using Reference.Api.Endpoints.PricingParameters.UpdatePricingParameter;

namespace Reference.Api.Endpoints.PricingParameters;

/// <summary>Pricing parameters of the Calculator sheet «Налаштування цін» (left table).</summary>
internal static class PricingParametersEndpoints
{
    public static IEndpointRouteBuilder MapPricingParametersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/pricing-parameters").WithTags("PricingParameters");
        GetPricingParametersEndpoint.Map(group);
        CreatePricingParameterEndpoint.Map(group);
        ImportPricingSettingsEndpoint.Map(group);
        UpdatePricingParameterEndpoint.Map(group);
        DeletePricingParameterEndpoint.Map(group);
        return app;
    }
}
