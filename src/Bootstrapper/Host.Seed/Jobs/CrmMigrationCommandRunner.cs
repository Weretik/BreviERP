using System.Text.Json;
using Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
namespace Host.Seed.Jobs;

internal static class CrmMigrationCommandRunner
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        var mode = configuration["crm-migrate"];
        if (mode is not ("plan" or "apply")) throw new InvalidOperationException("crm-migrate must be plan or apply.");
        var target = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("Default"));
        if (target.Host is not ("localhost" or "127.0.0.1" or "::1") || string.IsNullOrWhiteSpace(target.Database)
            || target.Database is "postgres" or "template0" or "template1")
            throw new InvalidOperationException("Explicit CRM migration mode is restricted to a named local application database.");
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        var hasHistory = await db.Database.SqlQueryRaw<bool>("""SELECT to_regclass('crm."__EFMigrationsHistory"') IS NOT NULL AS "Value" """).SingleAsync();
        var pending = hasHistory ? (await db.Database.GetPendingMigrationsAsync()).ToArray() : db.Database.GetMigrations().ToArray();
        if (mode == "apply") await db.Database.MigrateAsync();
        Console.WriteLine(JsonSerializer.Serialize(new { mode, database = target.Database, pendingBefore = pending,
            pendingAfter = mode == "apply" ? (await db.Database.GetPendingMigrationsAsync()).ToArray() : pending }));
    }
}
