using Crm.Application.Delivery.EnqueueQuote;
using Crm.Application.Delivery.GetDeliveries;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.Processing;
using Crm.Application.Delivery.Provider;
using Crm.Application.Delivery.Readiness;
using Crm.Infrastructure.Delivery.Gmail;
using Crm.Infrastructure.Delivery.Gmail.OAuth;
using Crm.Infrastructure.Delivery.Readiness;
using Crm.Infrastructure.Persistence.Delivery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmQuoteDeliveryServiceExtensions
{
    public static IServiceCollection AddCrmQuoteDelivery(this IServiceCollection services, IConfiguration configuration)
    {
        var delivery = configuration.GetSection(QuoteDeliveryProviderOptions.SectionName)
            .Get<QuoteDeliveryProviderOptions>() ?? new QuoteDeliveryProviderOptions();
        var oauth = configuration.GetSection(GmailOAuthOptions.SectionName)
            .Get<GmailOAuthOptions>() ?? new GmailOAuthOptions();
        services.AddSingleton(delivery);
        services.AddSingleton(oauth);
        // No redirects or retry policy: never forward credential forms or replay a send.
        services.AddHttpClient("Crm.GmailOAuth")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });
        services.AddSingleton<IQuoteDeliveryAccessTokenSource>(provider => new GmailOAuthAccessTokenSource(
            provider.GetRequiredService<IHttpClientFactory>().CreateClient("Crm.GmailOAuth"), oauth, delivery));
        services.AddHttpClient<GmailQuoteDeliveryProvider>()
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });
        services.AddTransient<IQuoteDeliveryProvider>(provider => provider.GetRequiredService<GmailQuoteDeliveryProvider>());
        // Presence permits enqueue; the source still verifies the actual Gmail profile before returning a token.
        services.AddSingleton<IQuoteDeliveryReadiness>(_ => new QuoteDeliveryReadinessPolicy(delivery,
            new QuoteDeliveryProviderCapabilities("gmail", true, true), credentialConfigured: oauth.IsConfigured));
        services.AddSingleton<IQuoteDeliveryIdentity, ConfiguredQuoteDeliveryIdentity>();
        services.AddScoped<IQuoteDeliveryStore, QuoteDeliveryStore>();
        services.AddScoped<EnqueueQuoteDeliveryHandler>();
        services.AddScoped<QuoteDeliveryReader>();
        services.AddScoped<QuoteDeliveryProcessor>();
        return services;
    }
}
