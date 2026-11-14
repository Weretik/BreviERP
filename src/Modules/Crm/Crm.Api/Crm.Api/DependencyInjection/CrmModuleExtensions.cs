using Crm.Api.Endpoints.Customers;
using Crm.Api.Endpoints.Embroidery;
using Crm.Api.Endpoints.Leads;
using Crm.Api.Endpoints.Quotes;
using Crm.Api.Endpoints.SiteRequests;
using Crm.Api.Endpoints.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Crm.Application;
using Crm.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Crm.Api;
using BuildingBlocks.Application.Validation;
using Crm.Api.Endpoints.Mailboxes;

namespace Crm.Api.DependencyInjection;

public static class CrmModuleExtensions
{
    /// <summary>Registers the Crm module: Application, Infrastructure, and its Contracts implementations.</summary>
    public static IServiceCollection AddCrmModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCrmInfrastructure(configuration);
        services.AddModuleValidators(typeof(CrmApplicationAssemblyMarker).Assembly);
        return services;
    }

    /// <summary>Maps the Crm module's HTTP endpoints.</summary>
    public static IEndpointRouteBuilder MapCrmEndpoints(this IEndpointRouteBuilder app) =>
        app.MapCustomerContactsEndpoints().MapCustomersEndpoints().MapEmbroideryEndpoints().MapLeadStatsEndpoints()
            .MapLeadsEndpoints().MapMailboxesEndpoints().MapQuoteImportEndpoints().MapQuoteIntroTemplatesEndpoints()
            .MapQuoteIntroductionsEndpoints().MapQuoteLookupsEndpoints().MapQuoteSettingsEndpoints()
            .MapQuotesEndpoints().MapPublicSiteRequestsEndpoints().MapSiteRequestsEndpoints()
            .MapManagerTasksEndpoints();
}
