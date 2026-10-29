using Crm.Infrastructure.Persistence.Customers.Writing;
using Crm.Infrastructure.Persistence.Customers.Reading;
using BuildingBlocks.Infrastructure.Migrations;
using Crm.Application.Customers.Persistence;
using Crm.Application.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Documents.Persistence;
using Crm.Application.Documents.Numbering;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.Source;
using Crm.Application.Abstractions.Documents;
using Crm.Infrastructure.Documents.Assets;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Crm.Infrastructure.Persistence.Documents.Reading;
using Crm.Infrastructure.Persistence.Documents;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Crm.Infrastructure.Persistence.Quotes.Reading;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Crm.Infrastructure.DependencyInjection;

public static class CrmInfrastructureExtensions
{
    public static IServiceCollection AddCrmDbContextServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));
        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<ICustomerMutationStore, CustomerMutationStore>();
        services.AddScoped<IQuotePriceBasisStore, QuotePriceBasisStore>();
        services.AddScoped<IQuoteMutationStore, QuoteMutationStore>();
        services.AddScoped<IQuoteReadRepository, QuoteReadRepository>();
        services.AddScoped<IDocumentSettingsStore, IssuerSettingsStore>();
        services.AddScoped<IDocumentNumberAllocator, DocumentNumberAllocator>();
        services.AddScoped<IQuotePdfArtifactStore, QuotePdfArtifactStore>();
        services.AddScoped<IQuoteDocumentSource, QuoteDocumentSource>();
        services.AddSingleton<IQuoteBrandAssetResolver>(_ =>
            new ConfiguredQuoteBrandAssetResolver(configuration["Crm:Documents:WordmarkPath"]));
        services.AddScoped<IQuoteCustomVersionReader, Crm.Infrastructure.Persistence.CustomSewing.Reading.QuoteCustomVersionReader>();
        services.AddScoped<QuoteVersionBuilder>();
        services.AddScoped<QuotePriceBasisResolver>();
        services.AddScoped<Crm.Application.CustomSewing.Persistence.ICustomMutationStore, Crm.Infrastructure.Persistence.CustomSewing.Writing.CustomMutationStore>();
        services.AddScoped<Crm.Application.CustomSewing.Persistence.ICustomReadRepository, Crm.Infrastructure.Persistence.CustomSewing.Reading.CustomReadRepository>();
        services.AddCrmTemplates();
        services.AddScoped<IDatabaseMigrator, DbMigrator<CrmDbContext>>();
        return services;
    }
}
