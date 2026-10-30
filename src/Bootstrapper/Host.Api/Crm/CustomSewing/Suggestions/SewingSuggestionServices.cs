using Catalog.Application.Contracts.Suggestions;
using Catalog.Infrastructure.Suggestions;
using Crm.Application.CustomSewing.GetSuggestions;
using Microsoft.Extensions.DependencyInjection;
namespace Host.Api.Crm.CustomSewing.Suggestions;

public static class SewingSuggestionServices
{
    public static IServiceCollection AddCrmSewingSuggestions(this IServiceCollection services)
    {
        services.AddScoped<ISewingSuggestionIdentityReader, SewingSuggestionIdentityReader>();
        services.AddScoped<ISewingSuggestionCompositionReader, SewingSuggestionCompositionReader>();
        services.AddScoped<ISewingSuggestionSource, SewingSuggestionSource>();
        return services;
    }
}
