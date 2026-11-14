using Ai.Contracts.Extraction;
using Ai.Infrastructure.Contracts.Extraction;
using Ai.Application.Abstractions;
using Ai.Application.DependencyInjection;
using Ai.Infrastructure.OpenAi;
using Ai.Infrastructure.Persistence;
using Ai.Infrastructure.Security;
using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ai.Infrastructure.DependencyInjection;

public static class AiInfrastructureExtensions
{
    /// <summary>DbContext, migrator, key protection and the OpenAI client. Data Protection keys are persisted by Identity.</summary>
    public static IServiceCollection AddAiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<AiDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", AiDbContext.Schema)));
        services.AddScoped<IAiDbContext>(sp => sp.GetRequiredService<AiDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<AiDbContext>>();

        services.AddDataProtection();
        services.TryAddSingleton<IAiKeyProtector, AiKeyProtector>();
        // A PDF of 20 MB and a slow model may take a while; the request carries the key, so no redirects.
        services.AddHttpClient<IOpenAiClient, OpenAiResponsesClient>(client =>
            {
                client.BaseAddress = new Uri(OpenAiResponsesClient.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(120);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.TryAddSingleton(TimeProvider.System);

        // Contracts other modules use (ADR-0006).
        services.AddScoped<IStructuredDataExtractor, StructuredDataExtractor>();
        services.AddAiApplication();
        return services;
    }
}
