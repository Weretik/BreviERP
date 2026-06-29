using Catalog.Infrastructure.Options;

namespace Catalog.Infrastructure.DependencyInjection;

public static class AdmToolsHttpClientRegistrationExtensions
{
    public const string HttpClientName = "AdmToolsStorage";

    public static IServiceCollection AddAdmToolsHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AdmToolsStorageOptions>()
            .Bind(configuration.GetSection(AdmToolsStorageOptions.SectionName))
            .Validate(x =>
                    !string.IsNullOrWhiteSpace(x.Host) &&
                    !string.IsNullOrWhiteSpace(x.Login) &&
                    !string.IsNullOrWhiteSpace(x.Password) &&
                    !string.IsNullOrWhiteSpace(x.PublicBaseUrl),
                "AdmToolsStorage options must be configured.");

        services.AddHttpClient(HttpClientName, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AdmToolsStorageOptions>>().Value;
            client.BaseAddress = new Uri($"https://{options.Host.Trim().TrimEnd('/')}");
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        return services;
    }
}
