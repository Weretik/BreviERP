using Reminders.Contracts.Telegram;
using Reminders.Infrastructure.Contracts.Telegram;
using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Reminders.Application.Abstractions;
using Reminders.Application.DependencyInjection;
using Reminders.Infrastructure.Persistence;
using Reminders.Infrastructure.Persistence.Seeding;
using Reminders.Infrastructure.Integrations.Telegram;
using Reminders.Application.Abstractions.Telegram;

namespace Reminders.Infrastructure.DependencyInjection;

public static class RemindersInfrastructureExtensions
{
    /// <summary>DbContext, migrator, Telegram client, seeder and shared application services.</summary>
    public static IServiceCollection AddRemindersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");
        services.AddDbContext<RemindersDbContext>(options => options.UseNpgsql(connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", RemindersDbContext.Schema)));
        services.AddScoped<IRemindersDbContext>(sp => sp.GetRequiredService<RemindersDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<RemindersDbContext>>();

        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.Section));
        services.AddSingleton<ITelegramWebhookSettings>(sp => sp.GetRequiredService<IOptions<TelegramOptions>>().Value);
        services.AddHttpClient<ITelegramGateway, TelegramBotApiGateway>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.TryAddSingleton(TimeProvider.System);
        services.AddRemindersApplication();

        // Contracts other modules use (ADR-0006).
        services.AddScoped<ITelegramBot, TelegramBot>();

        // Seeders run only in Host.Seed.
        services.AddScoped<ISeeder, DefaultReminderTaskSeeder>();
        return services;
    }
}
