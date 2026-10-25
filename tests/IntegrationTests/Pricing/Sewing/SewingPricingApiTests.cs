using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Ardalis.Result;
using Catalog.Api;
using Catalog.Application.Features.Product.Sewing;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Publications.GetProducts;
using Catalog.Application.Features.Pricing.Sewing.Settings.CreateVersion;
using Catalog.Application.Features.Pricing.Sewing.Publications.GetCurrent;
using Catalog.Application.Features.Pricing.Sewing.Publications.Publish;
using Catalog.Application.Features.Pricing.Sewing.Calculations.Save;
namespace IntegrationTests.Pricing.Sewing;

[NonParallelizable]
public sealed class SewingPricingApiTests
{
    private const string PricingPath = "/api/v1/products/42/articles/501/pricing";

    [Test]
    public async Task Batch_projection_requires_access_and_binds_repeated_product_ids()
    {
        await using var host = await TestHost.CreateAsync();
        const string route = "/api/v1/pricing/sewing/published?productIds=42&productIds=43";
        Assert.That((await host.Client.GetAsync(route)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        host.Client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        host.Sender.Setup(x => x.Send(It.IsAny<GetPublishedSewingProductsQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<IReadOnlyList<SewingPublishedProductPrices>>>(Result.Success<IReadOnlyList<SewingPublishedProductPrices>>([])));
        var response = await host.Client.GetAsync(route);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        host.Sender.Verify(x => x.Send(It.Is<GetPublishedSewingProductsQuery>(q => q.ProductIds.SequenceEqual(new[] { 42, 43 })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Protects_reads_writes_and_requires_actor_claims()
    {
        await using var host = await TestHost.CreateAsync();
        Assert.That((await host.Client.GetAsync(PricingPath + "/published")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        host.Client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        Assert.That((await host.Client.PostAsJsonAsync(PricingPath + "/calculations", new { requestId = Guid.NewGuid() })).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
        host.Client.DefaultRequestHeaders.Remove("X-Test-Role");
        host.Client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");
        Assert.That((await host.Client.PostAsJsonAsync(PricingPath + "/calculations", new { requestId = Guid.NewGuid() })).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
        host.Sender.Verify(x => x.Send(It.IsAny<SaveSewingCalculationCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Binds_scoped_ids_uses_claim_actor_and_serializes_published_tiers()
    {
        await using var host = await TestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");
        host.Client.DefaultRequestHeaders.Add("X-Test-UserId", "claim-manager");
        host.Sender.Setup(x => x.Send(It.IsAny<SaveSewingCalculationCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SavedSewingCalculation>>(Result.NotFound()));
        var requestId = Guid.NewGuid();
        Assert.That((await host.Client.PostAsJsonAsync(PricingPath + "/calculations",
            new { requestId, actor = "spoofed", at = "2000-01-01T00:00:00Z" })).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        host.Sender.Verify(x => x.Send(It.Is<SaveSewingCalculationCommand>(c => c.ProductId == 42 && c.ArticleId == 501
            && c.RequestId == requestId && c.Actor == "claim-manager"), It.IsAny<CancellationToken>()), Times.Once);
        var calculationId = Guid.NewGuid();
        host.Sender.Setup(x => x.Send(It.IsAny<PublishSewingCalculationCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PublishedSewingPrice>>(Result.Conflict("Stale calculation.")));
        Assert.That((await host.Client.PostAsync(PricingPath + $"/calculations/{calculationId}/publication", null)).StatusCode,
            Is.EqualTo(HttpStatusCode.Conflict));
        host.Sender.Verify(x => x.Send(It.Is<PublishSewingCalculationCommand>(c => c.CalculationId == calculationId
            && c.Actor == "claim-manager"), It.IsAny<CancellationToken>()), Times.Once);
        host.Sender.Setup(x => x.Send(It.IsAny<GetPublishedSewingPriceQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PublishedSewingPrice>>(Result.Success(new PublishedSewingPrice(
                Guid.NewGuid(), calculationId, 42, 501, DateTimeOffset.UtcNow, "claim-manager", new(100, 90, 80)))));
        var response = await host.Client.GetAsync(PricingPath + "/published");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.GetProperty("prices").GetProperty("largeWholesale").GetDecimal(), Is.EqualTo(80));
    }

    [Test]
    public async Task Incomplete_settings_are_rejected_before_mediator_and_reference_writes_are_protected()
    {
        await using var host = await TestHost.CreateAsync();
        Assert.That((await host.Client.PutAsJsonAsync("/api/reference/fabrics/1", new { })).StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized));
        host.Client.DefaultRequestHeaders.Add("X-Test-Role", "Manager");
        host.Client.DefaultRequestHeaders.Add("X-Test-UserId", "claim-manager");
        var response = await host.Client.PostAsJsonAsync("/api/v1/pricing/sewing/settings/versions",
            new { expectedVersionId = 1, requestId = Guid.NewGuid(), settings = new { monthlySalary = 17000 } });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<CreateSewingSettingsVersionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
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
            builder.Services.AddControllers().AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly)
                .AddApplicationPart(typeof(Reference.Api.Controllers.FabricsController).Assembly);

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
