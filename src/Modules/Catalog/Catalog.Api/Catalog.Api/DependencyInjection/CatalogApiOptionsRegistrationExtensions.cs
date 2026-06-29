using Catalog.Api.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Api.DependencyInjection;

public static class CatalogApiOptionsRegistrationExtensions
{
    public static IServiceCollection AddCatalogApiOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<CatalogMediaUploadOptions>()
            .Bind(configuration.GetSection(CatalogMediaUploadOptions.SectionName))
            .Validate(
                x => x.MaxFileSizeBytes > 0 && !string.IsNullOrWhiteSpace(x.BaseFolder),
                "CatalogMediaUpload:MaxFileSizeBytes must be greater than zero and BaseFolder must be configured.");

        services.Configure<FormOptions>(options =>
        {
            var configuredValue = configuration.GetValue<long?>($"{CatalogMediaUploadOptions.SectionName}:MaxFileSizeBytes");
            if (configuredValue.HasValue && configuredValue.Value > 0)
                options.MultipartBodyLengthLimit = configuredValue.Value;
        });

        return services;
    }
}
