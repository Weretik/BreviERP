using Crm.Application.CustomSewing.ApplyTemplate;
using Crm.Application.CustomSewing.Templates.Persistence;
using Crm.Infrastructure.Persistence.Templates.Reading;
using Crm.Infrastructure.Persistence.Templates.Writing;
using Microsoft.Extensions.DependencyInjection;
namespace Crm.Infrastructure.DependencyInjection;

public static class TemplateInfrastructureExtensions
{
    public static IServiceCollection AddCrmTemplates(this IServiceCollection services)
    {
        services.AddScoped<ITemplateReadStore, TemplateReadStore>();
        services.AddScoped<ITemplateMutationStore, TemplateMutationStore>();
        services.AddScoped<ITemplateApplySource, TemplateApplySource>();
        return services;
    }
}
