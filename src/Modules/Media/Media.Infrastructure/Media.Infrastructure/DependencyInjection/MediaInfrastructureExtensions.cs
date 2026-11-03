using BuildingBlocks.Infrastructure.Migrations;
using Media.Application.Abstractions;
using Media.Infrastructure.Persistence;
using Media.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Media.Infrastructure.DependencyInjection;

public static class MediaInfrastructureExtensions
{
    /// <summary>DbContext, migrator, CDN storage and upload limits of the photo library.</summary>
    public static IServiceCollection AddMediaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<MediaDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", MediaDbContext.Schema)));
        services.AddScoped<IMediaDbContext>(sp => sp.GetRequiredService<MediaDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<MediaDbContext>>();

        services.AddOptions<MediaUploadSettings>()
            .Bind(configuration.GetSection(MediaUploadSettings.Section))
            .Validate(x => x.MaxFileSizeBytes > 0 && !string.IsNullOrWhiteSpace(x.BaseFolder),
                "MediaUpload:MaxFileSizeBytes must be greater than zero and BaseFolder must be configured.");
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MediaUploadSettings>>().Value);

        services.AddAdmToolsHttpClient(configuration);
        services.AddSingleton<IMediaStorage, AdmToolsMediaStorage>();
        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
