using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Ardalis.Result;
using Crm.Api;
using Crm.Api.Customers.Requests;
using Crm.Application.Customers.CreateCustomer;
using Crm.Application.Customers.GetCustomers;
using Crm.Application.Customers.Models;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace IntegrationTests.Crm;

[NonParallelizable]
public sealed class CustomerApiTests
{
    [Test]
    public async Task AnonymousAndUserCannotReadAndManagerCannotMerge()
    {
        await using var host = await TestHost.CreateAsync();
        Assert.That((await host.Client.GetAsync("/api/crm/customers")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using (AsRole(host.Client, "User", "22"))
            Assert.That((await host.Client.GetAsync("/api/crm/customers")).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        using (AsRole(host.Client, "Manager", "22"))
            Assert.That((await host.Client.PostAsJsonAsync($"/api/crm/customers/{Guid.NewGuid()}/merge", new { })).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task ListPreservesValueAndPagedInfoAndRejectsInvalidPageBeforeSender()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<GetCustomersQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<PagedResult<IReadOnlyList<CustomerSummary>>>(new PagedResult<IReadOnlyList<CustomerSummary>>(new PagedInfo(2, 25, 3, 51), [])));
        using var actor = AsRole(host.Client, "Manager", "22");
        var response = await host.Client.GetAsync("/api/crm/customers?page=2");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json.GetProperty("value").GetArrayLength(), Is.Zero);
        Assert.That(json.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt64(), Is.EqualTo(51));
        Assert.That((await host.Client.GetAsync("/api/crm/customers?page=0")).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.Is<GetCustomersQuery>(q => q.Actor.UserId == 22 && q.Page == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreationUsesAuthenticatedActorAndReturnsCreatedLocation()
    {
        await using var host = await TestHost.CreateAsync();
        var id = Guid.NewGuid();
        host.Sender.Setup(x => x.Send(It.IsAny<CreateCustomerCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<CustomerDetail>>(Result.Success(new CustomerDetail(id, "person", "Synthetic", null, null,
                22, true, 1, DateTimeOffset.UtcNow, true, false, [], []))));
        using var actor = AsRole(host.Client, "Manager", "22");
        var response = await host.Client.PostAsJsonAsync("/api/crm/customers", new
        {
            requestId = Guid.NewGuid(), actor = new { userId = 999, isAdmin = true },
            input = new { kind = "person", displayName = "Synthetic", legalName = (string?)null, taxIdentity = (string?)null, externalIdentities = Array.Empty<object>() }
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location!.AbsolutePath, Is.EqualTo($"/api/crm/customers/{id}"));
        host.Sender.Verify(x => x.Send(It.Is<CreateCustomerCommand>(c => c.Actor.UserId == 22 && !c.Actor.IsAdmin), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static IDisposable AsRole(HttpClient client, string role, string userId)
    {
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        return new HeaderRemoval(client);
    }

    private sealed class HeaderRemoval(HttpClient client) : IDisposable
    {
        public void Dispose()
        {
            client.DefaultRequestHeaders.Remove("X-Test-Role");
            client.DefaultRequestHeaders.Remove("X-Test-UserId");
        }
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Role", out var role))
                return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new List<Claim> { new(ClaimTypes.Role, role.ToString()) };
            if (Request.Headers.TryGetValue("X-Test-UserId", out var userId))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            var identity = new ClaimsIdentity(claims, Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }

    private sealed class TestHost(WebApplication app, HttpClient client, Mock<ISender> sender) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;
        public Mock<ISender> Sender { get; } = sender;

        public static async Task<TestHost> CreateAsync()
        {
            var sender = new Mock<ISender>();
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddSingleton(sender.Object);
            builder.Services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("CatalogRead", policy => policy.RequireRole("User", "Manager", "Admin"));
                options.AddPolicy("CanManageProducts", policy => policy.RequireRole("Manager", "Admin"));
            });
            builder.Services.AddControllers().AddApplicationPart(typeof(CrmApiAssemblyMarker).Assembly);

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            await app.StartAsync();
            return new(app, app.GetTestClient(), sender);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }
}
