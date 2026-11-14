using Microsoft.Extensions.DependencyInjection;
using SewingPricing.Jobs.Reading;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Reference;

namespace SewingPricing.Jobs.Activation;

public static class ActivationServicesRegistration
{
    public static IServiceCollection AddPricingActivationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductReferenceReader, JobProductReferenceReader>();
        services.AddScoped<IPricingReferenceVersionReader, JobPricingReferenceReader>();
        services.AddScoped<SewingCompositionConfirmation>();
        services.AddScoped<PpeInputConfirmation>();
        services.AddScoped<SewingActivationPublisher>();
        return services;
    }
}
