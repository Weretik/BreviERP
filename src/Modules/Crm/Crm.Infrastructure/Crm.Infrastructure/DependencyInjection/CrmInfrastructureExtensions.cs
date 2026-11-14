using BuildingBlocks.Infrastructure.Outbox;
using Crm.Application.SiteRequests.Notify;
using Crm.Application.Embroidery.Mockup;
using Crm.Application.Embroidery.StitchRate;
using Crm.Application.Quotes.FabricFacts;
using Crm.Application.Quotes.Photos;
using Crm.Application.Quotes.Staff;
using Crm.Application.Tasks.Staff;
using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.CustomSewing;
using Crm.Application.Quotes.Import;
using Crm.Application.Quotes.ProductionLinks;
using Crm.Contracts.Customers;
using Crm.Contracts.Embroidery;
using Crm.Contracts.Quotes;
using Crm.Infrastructure.Adapters.Catalog;
using Crm.Infrastructure.Adapters.Identity;
using Crm.Infrastructure.Adapters.Production;
using Crm.Infrastructure.Contracts.Customers;
using Crm.Infrastructure.Contracts.Embroidery;
using Crm.Infrastructure.Contracts.Quotes;
using Crm.Infrastructure.Adapters.Media;
using Crm.Infrastructure.Adapters.Reference;
using Crm.Infrastructure.Adapters.Reminders;
using Crm.Application.SiteRequests.Abstractions;
using BuildingBlocks.Infrastructure.Migrations;
using Crm.Application.Customers.Persistence;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Leads.Persistence;
using Crm.Application.Leads.Sheet;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Introductions.Building;
using Crm.Application.Quotes.Pdf;
using Crm.Application.Quotes.Pdf.Composing;
using Crm.Application.Quotes.Sending.Gmail;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Tasks.Persistence;
using Crm.Infrastructure.Leads;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Customers.Reading;
using Crm.Infrastructure.Persistence.Customers.Writing;
using Crm.Infrastructure.Quotes.Pdf;
using Crm.Infrastructure.Quotes.Sending;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmInfrastructureExtensions
{
    public static IServiceCollection AddCrmDbContextServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));
        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<ICustomerMutationStore, CustomerMutationStore>();
        services.AddScoped<IQuoteStore>(provider => provider.GetRequiredService<CrmDbContext>());
        services.AddScoped<ITaskStore>(provider => provider.GetRequiredService<CrmDbContext>());
        services.AddScoped<IEmbroideryStore>(provider => provider.GetRequiredService<CrmDbContext>());
        services.AddScoped<ILeadStore>(provider => provider.GetRequiredService<CrmDbContext>());
        services.AddScoped<ISiteRequestStore>(provider => provider.GetRequiredService<CrmDbContext>());
        services.AddSingleton(provider =>
            new SiteRequestLinks(provider.GetRequiredService<IOptions<GmailOptions>>().Value.AdminUrl));
        services.Configure<LeadSheetOptions>(configuration.GetSection(LeadSheetOptions.Section));
        services.AddHttpClient<ILeadSheetReader, GoogleLeadSheetReader>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<LeadSheetSync>();
        services.AddScoped<QuoteItemsBuilder>();
        services.AddScoped<QuoteNumbering>();
        services.AddScoped<QuoteIntroductionBuilder>();
        services.AddHttpClient<QuotePdfImageLoader>(client => client.Timeout = TimeSpan.FromSeconds(10));
        services.AddScoped<IQuotePdfRenderer, QuotePdfRenderer>();
        services.AddScoped<QuotePdfComposer>();
        services.AddQuoteGmail(configuration);
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDatabaseMigrator, DbMigrator<CrmDbContext>>();
        services.AddOutbox<CrmDbContext>();
        services.AddOutboxHandler<NotifySiteRequestOutboxHandler>();

        // Adapters from other modules' Contracts to this module's ports.
        services.AddScoped<IQuoteStaffReader, QuoteStaffReader>();
        services.AddScoped<ITaskStaffReader, TaskStaffReader>();
        services.AddScoped<IEmbroideryStitchRateReader, EmbroideryStitchRateReader>();
        services.AddScoped<IQuoteFabricFactsReader, QuoteFabricFactsReader>();
        services.AddScoped<IEmbroideryMediaReader, EmbroideryMediaReader>();
        services.AddScoped<IQuotePhotoStorage, QuotePhotoStorage>();
        services.AddScoped<ISiteRequestFileStorage, SiteRequestFileStorage>();
        services.AddScoped<ISiteRequestNotifier, SiteRequestNotifier>();
        services.AddScoped<IQuoteCatalogReader, QuoteCatalogReader>();
        services.AddScoped<IQuoteProductionOrders, QuoteProductionOrders>();
        services.AddScoped<IQuoteSewingCalculator, QuoteSewingCalculator>();
        services.AddScoped<IQuoteModelCompositionReader, QuoteModelCompositionReader>();
        services.AddScoped<IQuoteImportLookup, QuoteImportLookup>();

        // Crm.Contracts for other modules.
        services.AddScoped<ICustomerDirectory, CustomerDirectory>();
        services.AddScoped<IQuoteReader, QuoteDirectory>();
        services.AddScoped<IEmbroideryCalculationReader, EmbroideryCalculationReader>();
        return services;
    }

    /// <summary>Gmail sending of КП (R-15). Data Protection is registered by the host (keys in PostgreSQL).</summary>
    private static void AddQuoteGmail(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GmailOptions>(configuration.GetSection(GmailOptions.Section));
        services.AddSingleton(provider =>
            new QuoteMailOptions(provider.GetRequiredService<IOptions<GmailOptions>>().Value.AdminUrl));
        // Requests carry the client secret and refresh token: no redirects; Gmail uploads may take a while.
        services.AddHttpClient<IGmailClient, GmailClient>(client => client.Timeout = TimeSpan.FromSeconds(60))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddDataProtection();
        services.AddSingleton<IMailboxProtector, MailboxProtector>();
    }
}
