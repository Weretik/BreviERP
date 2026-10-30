using Crm.Infrastructure.DependencyInjection;
using Crm.Infrastructure.Documents.Rendering;

namespace Host.Api.Crm.Documents;

public static class CrmDocumentServices
{
    public static IServiceCollection AddCrmQuoteDocuments(this IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment environment)
    {
        var executable = configuration["Crm:Documents:ChromiumPath"];
        var font = configuration["Crm:Documents:FontPath"];
        var workRoot = configuration["Crm:Documents:PrivateWorkingRootPath"];
        if (!string.IsNullOrWhiteSpace(executable) && !string.IsNullOrWhiteSpace(font) &&
            !string.IsNullOrWhiteSpace(workRoot))
            services.AddCrmPrivateDocumentRenderer(new ChromiumDocumentOptions
            {
                ExecutablePath = executable, FontPath = font,
                WorkingDirectoryRootPath = workRoot,
                PublicWebRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot")
            });
        services.AddHostedService<QuotePdfWorker>();
        return services;
    }
}
