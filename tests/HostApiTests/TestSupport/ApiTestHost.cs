using System.Security.Claims;
using System.Text.Encodings.Web;
using Host.Api.DependencyInjection.ServiceRegistration;
using Identity.Api.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HostApiTests.TestSupport;

/// <summary>
/// Every module's HTTP surface on a test server, without databases: the access rules and the endpoint list can be
/// checked; a request that passes authorization would need the module services and is not sent here.
/// </summary>
internal sealed class ApiTestHost(WebApplication app, HttpClient client) : IAsyncDisposable
{
    private static readonly Type[] ApiMarkers =
    [
        typeof(Accounting.Api.AccountingApiAssemblyMarker), typeof(Ai.Api.AiApiAssemblyMarker),
        typeof(Catalog.Api.CatalogApiAssemblyMarker), typeof(Content.Api.ContentApiAssemblyMarker),
        typeof(Crm.Api.CrmApiAssemblyMarker), typeof(Identity.Api.IdentityApiAssemblyMarker),
        typeof(Media.Api.MediaApiAssemblyMarker), typeof(Production.Api.ProductionApiAssemblyMarker),
        typeof(Purchasing.Api.PurchasingApiAssemblyMarker), typeof(Reference.Api.ReferenceApiAssemblyMarker),
        typeof(Reminders.Api.RemindersApiAssemblyMarker), typeof(Warehouse.Api.WarehouseApiAssemblyMarker)
    ];

    public HttpClient Client { get; } = client;

    public IServiceProvider Services => app.Services;

    /// <summary>The module HTTP endpoints: controller actions and mapped module endpoints.</summary>
    public RouteEndpoint[] ApiEndpoints() => ((IEndpointRouteBuilder)app).DataSources
        .SelectMany(source => source.Endpoints)
        .OfType<RouteEndpoint>()
        .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not null
                           || endpoint.Metadata.GetMetadata<HttpMethodMetadata>() is not null)
        .ToArray();

    /// <summary>"GET api/x/{id}": method and route without the leading slash and type constraints.</summary>
    public static string Describe(Endpoint endpoint)
    {
        var route = ((RouteEndpoint)endpoint).RoutePattern.RawText!.TrimStart('/').Replace(":int", string.Empty)
            .Replace(":long", string.Empty).Replace(":guid", string.Empty);
        var method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "ANY";
        return $"{method} {route}";
    }

    /// <param name="sender">Stands in for the mediator, so a request that passes authorization reaches its handler.</param>
    public static async Task<ApiTestHost> CreateAsync(Mediator.ISender? sender = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
        builder.WebHost.UseTestServer();
        builder.Services.AddAuthentication("Fixture")
            .AddScheme<AuthenticationSchemeOptions, FixtureAuthenticationHandler>("Fixture", _ => { });
        builder.Services.AddAuthorizationPolicies();
        if (sender is not null) builder.Services.AddSingleton(sender);
        // HTTP surface only: each module's Add<Module>Module() also needs a database.
        var mvc = builder.Services.AddControllers();
        foreach (var marker in ApiMarkers) mvc.AddApplicationPart(marker.Assembly);
        var app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapModuleEndpoints();
        await app.StartAsync();
        return new ApiTestHost(app, app.GetTestClient());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.DisposeAsync();
    }

    private sealed class FixtureAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Fixture-Role", out var role))
                return Task.FromResult(AuthenticateResult.NoResult());
            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "7"), new Claim(ClaimTypes.Role, role.ToString())], Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
