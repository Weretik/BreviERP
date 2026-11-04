using BuildingBlocks.Infrastructure.Migrations;
using Crm.Application.Customers.Persistence;
using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Introductions.Building;
using Crm.Application.Quotes.Pdf;
using Crm.Application.Quotes.Pdf.Composing;
using Crm.Application.Quotes.Sending.Gmail;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Tasks.Persistence;
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
        services.AddScoped<QuoteItemsBuilder>();
        services.AddScoped<QuoteNumbering>();
        services.AddScoped<QuoteIntroductionBuilder>();
        services.AddHttpClient<QuotePdfImageLoader>(client => client.Timeout = TimeSpan.FromSeconds(10));
        services.AddScoped<IQuotePdfRenderer, QuotePdfRenderer>();
        services.AddScoped<QuotePdfComposer>();
        services.AddQuoteGmail(configuration);
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDatabaseMigrator, DbMigrator<CrmDbContext>>();
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
