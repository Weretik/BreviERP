using Media.Contracts.Files;
using Media.Contracts.Imports;
using Media.Contracts.Storage;
using Media.Infrastructure.Contracts.Files;
using Media.Infrastructure.Contracts.Imports;
using Media.Infrastructure.Contracts.Storage;
using Media.Application.Abstractions;
using Media.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Media.Application.Features.Files.Shared.Images;
using Media.Infrastructure.Integrations.AdmTools;
using Media.Infrastructure.Integrations.Skia;
using BuildingBlocks.Infrastructure.Database;

namespace Media.Infrastructure.DependencyInjection;

public static class MediaInfrastructureExtensions
{
    /// <summary>DbContext, migrator, CDN storage, previews and upload limits of the photo library.</summary>
    public static IServiceCollection AddMediaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<MediaDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", MediaDbContext.Schema)));
        services.AddScoped<IMediaDbContext>(sp => sp.GetRequiredService<MediaDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<MediaDbContext>>();

        services.AddOptions<MediaUploadSettings>()
            .Bind(configuration.GetSection(MediaUploadSettings.Section))
            .Validate(x => x.MaxFileSizeBytes > 0 && !string.IsNullOrWhiteSpace(x.BaseFolder)
                    && !string.IsNullOrWhiteSpace(x.ThumbnailFolder) && x.ThumbnailMaxSize > 0,
                "MediaUpload:MaxFileSizeBytes and ThumbnailMaxSize must be greater than zero; BaseFolder and ThumbnailFolder must be configured.");
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MediaUploadSettings>>().Value);

        services.AddAdmToolsHttpClient(configuration);
        services.AddSingleton<IMediaStorage, AdmToolsMediaStorage>();
        services.AddSingleton<IMediaImageProcessor, SkiaMediaImageProcessor>();
        services.AddScoped<MediaImagePreparer>();
        services.AddScoped<MediaImageBackfill>();
        services.TryAddSingleton(TimeProvider.System);

        // Contracts other modules use (ADR-0006).
        services.AddScoped<IMediaFileReader, MediaFileReader>();
        services.AddScoped<IMediaFileUploader, MediaFileUploader>();
        services.AddSingleton<ICdnStorage, CdnStorage>();
        services.AddHttpClient<IMediaUrlImporter, MediaUrlImporter>(client => client.Timeout = TimeSpan.FromSeconds(30));

        // Adapters from other modules' Contracts to this module's ports.
        services.AddScoped<IMediaUsageReader, Media.Infrastructure.Adapters.Catalog.MediaUsageReader>();
        return services;
    }
}
