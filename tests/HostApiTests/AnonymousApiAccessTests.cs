using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Host.Api.DependencyInjection.ServiceRegistration.Web;
using Host.Api.Security.AnonymousApiAccess;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HostApiTests;

public sealed class AnonymousApiAccessTests
{
    [TestCase("GET")]
    [TestCase("POST")]
    [TestCase("PUT")]
    [TestCase("DELETE")]
    public async Task Enabled_mode_allows_all_methods_and_supplies_real_admin_actor(string method)
    {
        await using var host = await Fixture.CreateAsync(true);
        using var request = new HttpRequestMessage(new HttpMethod(method), "/api/access-fixture/admin");
        var response = await host.Client.SendAsync(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("42:True"));
    }

    [Test]
    public async Task Enabled_mode_ignores_unusable_bearer_and_named_policy()
    {
        await using var host = await Fixture.CreateAsync(true);
        host.Client.DefaultRequestHeaders.Authorization = new("Bearer", "invalid-fixture");
        var response = await host.Client.GetAsync("/api/access-fixture/named");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [TestCase(true, HttpStatusCode.OK)]
    [TestCase(false, HttpStatusCode.Unauthorized)]
    public async Task Fallback_policy_respects_temporary_switch(bool enabled, HttpStatusCode expected)
    {
        await using var host = await Fixture.CreateAsync(enabled);
        Assert.That((await host.Client.GetAsync("/api/access-fixture/fallback")).StatusCode, Is.EqualTo(expected));
    }

    [Test]
    public async Task Disabled_mode_preserves_protected_action_and_does_not_resolve_actor()
    {
        await using var host = await Fixture.CreateAsync(false);
        Assert.That((await host.Client.GetAsync("/api/access-fixture/admin")).StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized));
        Assert.That(host.Resolver.Calls, Is.Zero);
    }

    [TestCase(true, HttpStatusCode.OK)]
    [TestCase(false, HttpStatusCode.Forbidden)]
    public async Task Existing_lower_privilege_identity_respects_selected_mode(bool enabled, HttpStatusCode expected)
    {
        await using var host = await Fixture.CreateAsync(enabled);
        host.Client.DefaultRequestHeaders.Add("X-Fixture-Role", "Manager");
        var response = await host.Client.GetAsync("/api/access-fixture/admin");
        Assert.That(response.StatusCode, Is.EqualTo(expected));
        if (enabled)
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("42:True"));
        else
            Assert.That(host.Resolver.Calls, Is.Zero);
    }

    [Test]
    public async Task Missing_admin_returns_stable_unavailable_instead_of_invented_user_id()
    {
        await using var host = await Fixture.CreateAsync(true, missingActor: true);
        var response = await host.Client.GetAsync("/api/access-fixture/admin");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("anonymous_api_actor_unavailable"));
    }

    [Test]
    public async Task Already_public_action_does_not_require_admin_lookup()
    {
        await using var host = await Fixture.CreateAsync(true, missingActor: true);
        Assert.That((await host.Client.GetAsync("/api/access-fixture/public")).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(host.Resolver.Calls, Is.Zero);
    }

    [Test]
    public async Task Every_real_module_controller_endpoint_is_anonymous_when_enabled()
    {
        await using var host = await Fixture.CreateAsync(true);
        var endpoints = ((IEndpointRouteBuilder)host.App).DataSources.SelectMany(source => source.Endpoints)
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not null).ToArray();
        TestContext.WriteLine($"Controller endpoint inventory: {endpoints.Length}");
        Assert.That(endpoints.Length, Is.GreaterThan(50));
        Assert.That(endpoints.Where(endpoint => endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null), Is.Empty);
    }

    [TestCase(true, 0)]
    [TestCase(false, 1)]
    public async Task Generated_swagger_security_matches_access_mode(bool enabled, int expectedRequirements)
    {
        await using var host = await Fixture.CreateAsync(enabled);
        var options = host.App.Services.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;
        Assert.That(options.SwaggerGeneratorOptions.SecurityRequirements.Count, Is.EqualTo(expectedRequirements));
    }

    [Test]
    public async Task Production_documentation_is_public_without_resolving_a_user()
    {
        await using var host = await Fixture.CreateAsync(true, missingActor: true);
        foreach (var url in new[] { "/swagger/index.html", "/swagger/v1/swagger.json", "/openapi/v1.json" })
        {
            var response = await host.Client.GetAsync(url);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), url);
        }
        Assert.That(host.Resolver.Calls, Is.Zero);
    }

    private sealed class Fixture(WebApplication app, HttpClient client, ActorResolver resolver) : IAsyncDisposable
    {
        public WebApplication App { get; } = app;
        public HttpClient Client { get; } = client;
        public ActorResolver Resolver { get; } = resolver;

        public static async Task<Fixture> CreateAsync(bool enabled, bool missingActor = false)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Production" });
            builder.WebHost.UseTestServer();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
                { ["ApiAccess:AnonymousEnabled"] = enabled.ToString() });
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddAuthentication("Fixture")
                .AddScheme<AuthenticationSchemeOptions, NoCredentialsHandler>("Fixture", _ => { });
            builder.Services.AddAuthorizationPolicies();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerDocumentation();
            builder.Services.AddOpenApi();
            builder.Services.AddAnonymousApiAccess(builder.Configuration);
            var resolver = new ActorResolver(missingActor);
            builder.Services.AddScoped<IAnonymousApiActorResolver>(_ => resolver);
            builder.Services.AddModuleControllers().AddApplicationPart(typeof(AnonymousAccessFixtureController).Assembly);
            var app = builder.Build();
            app.UseApiDocumentation();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAnonymousApiAccess();
            app.UseAuthorization();
            app.MapApiControllers();
            await app.StartAsync();
            return new Fixture(app, app.GetTestClient(), resolver);
        }

        public async ValueTask DisposeAsync() { Client.Dispose(); await App.DisposeAsync(); }
    }

    private sealed class ActorResolver(bool missing) : IAnonymousApiActorResolver
    {
        public int Calls { get; private set; }
        public Task<AnonymousApiActor?> ResolveAsync(CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(missing ? null : new AnonymousApiActor(42, "Fixture Admin", "admin@example.test"));
        }
    }

    private sealed class NoCredentialsHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
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

[ApiController]
[Route("api/access-fixture")]
public sealed class AnonymousAccessFixtureController : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [AcceptVerbs("GET", "POST", "PUT", "DELETE", Route = "admin")]
    public IActionResult AdminAction() => Content($"{User.FindFirstValue(ClaimTypes.NameIdentifier)}:{User.IsInRole("Admin")}");

    [Authorize(Policy = "CanManageProducts")]
    [HttpGet("named")]
    public IActionResult Named() => Ok();

    [HttpGet("fallback")]
    public IActionResult Fallback() => Ok();

    [AllowAnonymous]
    [HttpGet("public")]
    public IActionResult Public() => Ok();
}
