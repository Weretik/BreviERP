using BuildingBlocks.Infrastructure.Outbox;
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
using BuildingBlocks.Infrastructure.Migrations;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Leads.Shared.Persistence;
using Crm.Application.Features.Leads.Shared.Sheet;
using Crm.Application.Features.Quotes.Commands.ImportQuotes;
using Crm.Application.Features.Quotes.Commands.UploadQuotePhoto;
using Crm.Application.Features.Quotes.Queries.GetQuoteFabricFacts;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Building;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Introductions.Building;
using Crm.Application.Features.Quotes.Shared.Pdf;
using Crm.Application.Features.Quotes.Shared.Pdf.Composing;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Features.Quotes.Shared.Sending.Gmail;
using Crm.Application.Features.SiteRequests.Shared.Notify;
using Crm.Application.Features.Tasks.Shared.Persistence;
using Crm.Application.Abstractions.Customers;
using Crm.Application.Abstractions.Embroidery;
using Crm.Application.Abstractions.Leads;
using Crm.Application.Abstractions.Quotes;
using Crm.Application.Abstractions.SiteRequests;
using Crm.Application.Abstractions.Tasks;
using Crm.Infrastructure.Integrations.Gmail;
using Crm.Infrastructure.Integrations.GoogleSheets;
using Crm.Infrastructure.Integrations.Pdf;
using Crm.Infrastructure.Persistence.Customers;

namespace Crm.Infrastructure.DependencyInjection;

public static class CrmInfrastructureExtensions
{
    public static IServiceCollection AddCrmInfrastructure(this IServiceCollection services, IConfiguration configuration)
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
        services.AddCrmKeepinCrmSync(configuration);
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
