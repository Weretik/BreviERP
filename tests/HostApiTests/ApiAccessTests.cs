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
/// Access rules of every real module controller: only sign-in, the Telegram webhook and the public storefront
/// categories are open; everything else needs a signed-in user, and admin data needs a staff role.
/// </summary>
public sealed class ApiAccessTests
{
    private static readonly string[] PublicEndpoints =
    [
        "POST api/auth/cookie/login",
        "POST api/reminders/telegram/webhook",
        "GET api/reference/product-categories/store/{language}"
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
        string[] signedInOnly = ["GET api/auth/session/me", "POST api/auth/cookie/logout"];

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

    private static string Describe(Endpoint endpoint)
    {
        var route = ((RouteEndpoint)endpoint).RoutePattern.RawText!.Replace(":int", string.Empty);
        var method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault() ?? "ANY";
        return $"{method} {route}";
    }

    private sealed class Fixture(WebApplication app, HttpClient client) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;

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
