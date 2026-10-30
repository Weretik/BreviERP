using Crm.Application.Documents.EmailTemplates;
using Crm.Infrastructure.Persistence.Documents.EmailTemplates;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmEmailTemplateServiceExtensions
{
    public static IServiceCollection AddCrmEmailTemplates(this IServiceCollection services)
    {
        services.AddScoped<EmailTemplateStore>();
        services.AddScoped<IEmailTemplateStore>(provider => provider.GetRequiredService<EmailTemplateStore>());
        services.AddScoped<IEmailTemplateVersionReader>(provider => provider.GetRequiredService<EmailTemplateStore>());
        services.AddScoped<EmailTemplateService>();
        return services;
    }
}
