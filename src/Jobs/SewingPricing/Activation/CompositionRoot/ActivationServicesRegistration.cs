using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Reference;
using Microsoft.Extensions.DependencyInjection;
using SewingPricing.Jobs.Reading;

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
