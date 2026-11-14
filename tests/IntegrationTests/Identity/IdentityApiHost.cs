using BuildingBlocks.Application.DependencyInjection;
using Identity.Api.DependencyInjection;
using Host.Api.DependencyInjection.ServiceRegistration.Pipeline;
using Host.Api.DependencyInjection.ServiceRegistration.Web;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.DependencyInjection;
using Identity.Infrastructure.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace IntegrationTests.Identity;

/// <summary>
/// Real Identity module on a throwaway PostgreSQL database, with the Host.Api CSRF, authentication and
/// authorization pipeline. Clients keep cookies like a browser.
/// </summary>
internal sealed class IdentityApiHost : IAsyncDisposable
{
    public const string AdminEmail = "admin@brevi.test";
    public const string AdminPassword = "Admin-Password-1";

    private readonly string _databaseName;
    private readonly string _maintenanceConnectionString;

    private IdentityApiHost(WebApplication app, string databaseName, string maintenanceConnectionString)
    {
        App = app;
        _databaseName = databaseName;
        _maintenanceConnectionString = maintenanceConnectionString;
    }

    public WebApplication App { get; }

    /// <summary>Avatar files the API saved, instead of the CDN.</summary>
    public InMemoryAvatarStorage Avatars { get; private init; } = null!;

    public static async Task<IdentityApiHost> CreateAsync()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(baseConnectionString, Is.Not.Null.And.Not.Empty,
            "Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string with CREATE DATABASE permission.");

        var databaseName = $"brevierp_identity_{Guid.NewGuid():N}";
        var connection = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = databaseName }.ConnectionString;
        var maintenance = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = "postgres" }.ConnectionString;
        await ExecuteAsync(maintenance, $"CREATE DATABASE \"{databaseName}\"");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = connection,
            ["Identity:AdminUser:Email"] = AdminEmail,
            ["Identity:AdminUser:FullName"] = "Test Admin",
            ["ADMIN_DEFAULT_PASSWORD"] = AdminPassword
        });
        builder.Services.AddIdentityConfiguration(builder.Configuration);
        var avatars = new InMemoryAvatarStorage();
        builder.Services.AddSingleton<global::Identity.Application.Abstractions.IAvatarStorage>(avatars);
        builder.Services.AddIdentityInfrastructureServices(builder.Configuration);
        builder.Services.AddMediatorPipeline();
        builder.Services.AddModuleValidators(typeof(global::Identity.Application.IdentityApplicationAssemblyMarker).Assembly);
        builder.Services.AddAuthorizationPolicies();
        // Crm and Production controllers too: the access tests check that a role without the right gets 403 there.
        builder.Services.AddControllers().AddApplicationPart(typeof(global::Identity.Api.IdentityApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(global::Crm.Api.CrmApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(global::Production.Api.ProductionApiAssemblyMarker).Assembly);

        var app = builder.Build();
        app.UseCookieCsrfProtection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await using (var scope = app.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>().Database.MigrateAsync();
            // Roles first, then the administrator, the same order the seed job uses.
            foreach (var seeder in scope.ServiceProvider.GetServices<IIdentitySeeder>())
                await seeder.SeedAsync(scope.ServiceProvider);
        }

        await app.StartAsync();
        return new IdentityApiHost(app, databaseName, maintenance) { Avatars = avatars };
    }

    /// <summary>A new browser: its own cookie jar, HTTPS so Secure and __Host- cookies are kept.</summary>
    public BrowserClient CreateBrowser() => new(App.GetTestServer());

    public async ValueTask DisposeAsync()
    {
        await App.DisposeAsync();
        NpgsqlConnection.ClearAllPools();
        await ExecuteAsync(_maintenanceConnectionString, $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)");
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

internal sealed class InMemoryAvatarStorage : global::Identity.Application.Abstractions.IAvatarStorage
{
    public System.Collections.Concurrent.ConcurrentDictionary<string, byte[]> Files { get; } = new();

    public bool IsConfigured => true;

    public Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default)
    {
        Files[key] = content;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        Files.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public string? BuildUrl(string? key) => key is null ? null : $"https://cdn.brevi.test/avatars/{key}";
}

internal sealed class BrowserClient : IDisposable
{
    private readonly Dictionary<string, string> _cookies = new(StringComparer.Ordinal);

    public BrowserClient(TestServer server)
    {
        Http = new HttpClient(new CookieHandler(_cookies) { InnerHandler = server.CreateHandler() })
        {
            BaseAddress = new Uri("https://api.brevi.test")
        };
    }

    public HttpClient Http { get; }

    public string? Cookie(string name) => _cookies.GetValueOrDefault(name);

    public void Dispose() => Http.Dispose();

    /// <summary>A minimal cookie jar: one host, keeps name=value, drops cookies the server expires.</summary>
    private sealed class CookieHandler(Dictionary<string, string> cookies) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (cookies.Count > 0)
                request.Headers.Add("Cookie", string.Join("; ", cookies.Select(cookie => $"{cookie.Key}={cookie.Value}")));

            var response = await base.SendAsync(request, cancellationToken);
            if (!response.Headers.TryGetValues("Set-Cookie", out var setCookies))
                return response;

            foreach (var setCookie in setCookies)
            {
                var parts = setCookie.Split(';', StringSplitOptions.TrimEntries);
                var separator = parts[0].IndexOf('=');
                var name = parts[0][..separator];
                var value = parts[0][(separator + 1)..];
                var expired = parts.Any(part => part.StartsWith("expires=", StringComparison.OrdinalIgnoreCase) &&
                    DateTimeOffset.TryParse(part[8..], out var expires) && expires < DateTimeOffset.UtcNow);
                if (expired || value.Length == 0)
                    cookies.Remove(name);
                else
                    cookies[name] = value;
            }

            return response;
        }
    }
}
