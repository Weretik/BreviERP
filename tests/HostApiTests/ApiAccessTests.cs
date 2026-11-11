using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Host.Api.DependencyInjection.ServiceRegistration.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HostApiTests;

/// <summary>
/// Access rules of every real module controller: only sign-in, the Telegram webhook, the Gmail consent callback and the
/// public storefront categories and the public site catalog are open; everything else needs a signed-in user, and admin data needs a staff role.
/// </summary>
public sealed class ApiAccessTests
{
    private static readonly string[] PublicEndpoints =
    [
        "POST api/auth/cookie/login",
        "POST api/reminders/telegram/webhook",
        "GET api/crm/mailboxes/google-callback",
        "GET api/reference/product-categories/store/{language}",
        "GET api/public/catalog/tree",
        "GET api/public/catalog/products",
        "GET api/public/catalog/products/{slug}",
        "GET api/public/catalog/product-slugs",
        "GET api/public/catalog/categories",
        "GET api/public/catalog/categories/{slug}",
        "GET api/public/catalog/category-slugs",
        "GET api/public/articles",
        "GET api/public/articles/{slug}",
        "GET api/public/redirects",
        "GET api/public/site-settings"
    ];

    [Test]
    public async Task Only_the_listed_endpoints_are_anonymous()
    {
        await using var host = await Fixture.CreateAsync();

        var anonymous = host.ControllerEndpoints()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .Select(Describe)
            .ToArray();

        TestContext.WriteLine($"Controller endpoints: {host.ControllerEndpoints().Length}");
        Assert.That(host.ControllerEndpoints().Length, Is.GreaterThan(50));
        Assert.That(anonymous, Is.EquivalentTo(PublicEndpoints));
    }

    [Test]
    public async Task Every_other_endpoint_needs_a_role_or_policy_except_own_session_actions()
    {
        await using var host = await Fixture.CreateAsync();
        string[] signedInOnly = ["GET api/auth/session/me", "GET api/auth/session/access", "POST api/auth/cookie/logout"];

        var withoutStaffRule = host.ControllerEndpoints()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null)
            .Where(endpoint => !endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>()
                .Any(data => !string.IsNullOrEmpty(data.Policy) || !string.IsNullOrEmpty(data.Roles)))
            .Select(Describe)
            .Except(signedInOnly)
            .ToArray();

        Assert.That(withoutStaffRule, Is.Empty);
    }

    [TestCase(null, HttpStatusCode.Unauthorized)]
    [TestCase("User", HttpStatusCode.Forbidden)]
    public async Task Admin_catalogue_write_rejects_anonymous_and_non_staff(string? role, HttpStatusCode expected)
    {
        await using var host = await Fixture.CreateAsync();
        if (role is not null)
            host.Client.DefaultRequestHeaders.Add("X-Fixture-Role", role);

        var response = await host.Client.DeleteAsync("/api/reference/suppliers/1");

        Assert.That(response.StatusCode, Is.EqualTo(expected));
    }

    [Test]
    public async Task Manager_cannot_open_the_users_page_api()
    {
        await using var host = await Fixture.CreateAsync();
        host.Client.DefaultRequestHeaders.Add("X-Fixture-Role", "Manager");

        var response = await host.Client.GetAsync("/api/admin/users");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [TestCase("Accountant")]
    [TestCase("Admin")]
    [TestCase("Manager")]
    public async Task Only_the_director_opens_purchase_invoices(string role)
    {
        await using var host = await Fixture.CreateAsync();
        host.Client.DefaultRequestHeaders.Add("X-Fixture-Role", role);

        var response = await host.Client.GetAsync("/api/purchasing/invoices/sync-state");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Every_policy_an_endpoint_names_exists()
    {
        await using var host = await Fixture.CreateAsync();
        var provider = host.Services.GetRequiredService<IAuthorizationPolicyProvider>();
        var names = host.ControllerEndpoints()
            .SelectMany(endpoint => endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>())
            .Select(data => data.Policy)
            .OfType<string>()
            .Distinct()
            .ToArray();

        var missing = new List<string>();
        foreach (var name in names)
        {
            if (await provider.GetPolicyAsync(name) is null) missing.Add(name);
        }

        Assert.That(missing, Is.Empty);
    }

    // «Права доступу» defaults keep the old rules (client corrections item 11).
    [TestCase("WorkshopChief", "GET", "/api/crm/quotes")]
    [TestCase("Manager", "GET", "/api/production/staff")]
    [TestCase("Accountant", "GET", "/api/crm/tasks")]
    [TestCase("Admin", "DELETE", "/api/crm/quotes/1?version=1")]
    [TestCase("Manager", "GET", "/api/crm/quotes/legacy-archive")]
    [TestCase("Admin", "GET", "/api/accounting/overview")]
    [TestCase("custom-1a2b3c", "GET", "/api/crm/quotes")]
    [TestCase("Manager", "GET", "/api/admin/access")]
    public async Task A_role_without_the_right_gets_403(string role, string method, string path)
    {
        await using var host = await Fixture.CreateAsync();
        host.Client.DefaultRequestHeaders.Add("X-Fixture-Role", role);

        var response = await host.Client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    private static string Describe(Endpoint endpoint)
    {
        var route = ((RouteEndpoint)endpoint).RoutePattern.RawText!.Replace(":int", string.Empty);
        var method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "ANY";
        return $"{method} {route}";
    }

    private sealed class Fixture(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

        public IServiceProvider Services => app.Services;

        public Endpoint[] ControllerEndpoints() => ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not null)
            .ToArray();

        public static async Task<Fixture> CreateAsync()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
            builder.WebHost.UseTestServer();
            builder.Services.AddAuthentication("Fixture")
                .AddScheme<AuthenticationSchemeOptions, FixtureAuthenticationHandler>("Fixture", _ => { });
            builder.Services.AddAuthorizationPolicies();
            builder.Services.AddModuleControllers();
            var app = builder.Build();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            await app.StartAsync();
            return new Fixture(app, app.GetTestClient());
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
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
