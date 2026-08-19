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
        var section = configuration.GetRequiredSection(CatalogMediaUploadOptions.SectionName);
        var maxFileSizeBytes = section.GetValue<long?>(nameof(CatalogMediaUploadOptions.MaxFileSizeBytes)) ?? 0;

        services
            .AddOptions<CatalogMediaUploadOptions>()
            .Bind(section)
            .Validate(
                x => x.MaxFileSizeBytes > 0 && !string.IsNullOrWhiteSpace(x.BaseFolder),
                "CatalogMediaUpload:MaxFileSizeBytes must be greater than zero and BaseFolder must be configured.")
            .ValidateOnStart();

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = maxFileSizeBytes;
        });

        return services;
    }
}
