using Crm.Application.Abstractions.Documents;
using Crm.Application.Abstractions.Files;
using Crm.Infrastructure.Documents.Rendering;
using Crm.Infrastructure.Files.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmPrivateAdaptersExtensions
{
    public static IServiceCollection AddCrmPrivateFileStorage(this IServiceCollection services, PrivateFileStorageOptions options)
    {
        services.AddSingleton<IPrivateFileStore>(_ => new FileSystemPrivateFileStore(options));
        return services;
    }

    public static IServiceCollection AddCrmPrivateDocumentRenderer(this IServiceCollection services, ChromiumDocumentOptions options)
    {
        services.AddSingleton<IPrivateDocumentRenderer>(_ => new ChromiumPrivateDocumentRenderer(options));
        return services;
    }
}
