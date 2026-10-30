using Crm.Application.Abstractions.Files;
using Crm.Application.Files.Access;
using Crm.Application.Files.Cleanup;
using Crm.Application.Files.Finalize;
using Crm.Application.Files.Reading;
using Crm.Application.Files.Storage;
using Crm.Application.Files.Upload;
using Crm.Application.Files.Verification;
using Crm.Infrastructure.Files.Access;
using Crm.Infrastructure.Files.Cleanup;
using Crm.Infrastructure.Files.Finalize;
using Crm.Infrastructure.Files.Reading;
using Crm.Infrastructure.Files.Storage;
using Crm.Infrastructure.Files.Upload;
using Crm.Infrastructure.Files.Verification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmPrivateFileLifecycleExtensions
{
    public static IServiceCollection AddCrmPrivateFileLifecycle(this IServiceCollection services,
        PrivateFileStorageOptions storage, PrivateUploadOptions? uploads = null)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(storage);
        services.AddSingleton(uploads ?? new PrivateUploadOptions());
        services.AddSingleton<ConfiguredPrivateFileStore>();
        services.AddSingleton<IPrivateFileStore>(provider => provider.GetRequiredService<ConfiguredPrivateFileStore>());
        services.AddSingleton<IReservedPrivateFileStore>(provider => provider.GetRequiredService<ConfiguredPrivateFileStore>());
        services.AddScoped<IPrivateFileAccess, CustomPrivateFileAccess>();
        services.AddScoped<PrivateFileService>();
        services.AddScoped<ICustomFileUploader, CustomFileUploader>();
        services.AddScoped<IFinalizedCustomFilesWriter, FinalizedCustomFilesWriter>();
        services.AddScoped<ICustomFileReader, CustomFileReader>();
        services.AddScoped<ICustomArtworkVerifier, CustomArtworkVerifier>();
        services.AddScoped<ICustomTemporaryFileCleanup, CustomTemporaryFileCleanup>();
        return services;
    }
}
