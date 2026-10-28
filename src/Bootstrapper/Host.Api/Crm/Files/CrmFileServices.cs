using System.Globalization;
using Crm.Infrastructure.DependencyInjection;
using Crm.Infrastructure.Files.Storage;
using Crm.Infrastructure.Files.Upload;

namespace Host.Api.Crm.Files;

public static class CrmFileServices
{
    public static IServiceCollection AddCrmPrivateFiles(this IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredLifetime = configuration["Crm:PrivateFiles:TemporaryLifetimeMinutes"];
        var lifetime = configuredLifetime is null ? TimeSpan.FromHours(24)
            : int.TryParse(configuredLifetime, NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes)
                ? TimeSpan.FromMinutes(minutes) : TimeSpan.Zero;
        return services.AddCrmPrivateFileLifecycle(new PrivateFileStorageOptions
        {
            RootPath = configuration["Crm:PrivateFiles:RootPath"] ?? string.Empty,
            PublicWebRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot")
        }, new PrivateUploadOptions { TemporaryLifetime = lifetime });
    }
}
