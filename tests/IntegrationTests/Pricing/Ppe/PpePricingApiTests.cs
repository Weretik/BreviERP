using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Products;
using Catalog.Application.Features.Pricing.Ppe.Offers.CreateVersion;
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
namespace IntegrationTests.Pricing.Ppe;

[NonParallelizable]
public sealed class PpePricingApiTests
{
    private const string PricingPath = "/api/v1/products/42/articles/501/pricing";

    [Test]
    public async Task Ppe_reads_require_auth_and_writes_use_claim_actor_and_expected_version()
    {
        await using var host=await TestHost.CreateAsync();
        const string path="/api/v1/pricing/ppe/products";
        Assert.That((await host.Client.GetAsync(path+"?productIds=42")).StatusCode,Is.EqualTo(HttpStatusCode.Unauthorized));
        host.Client.DefaultRequestHeaders.Add("X-Test-Role","User");
        Assert.That((await host.Client.PostAsJsonAsync(path+"/42/offer/versions",new {})).StatusCode,Is.EqualTo(HttpStatusCode.Forbidden));
        host.Client.DefaultRequestHeaders.Remove("X-Test-Role");host.Client.DefaultRequestHeaders.Add("X-Test-Role","Manager");
        host.Client.DefaultRequestHeaders.Add("X-Test-UserId","claim-manager");
        host.Sender.Setup(x=>x.Send(It.IsAny<GetPpeProductPricesQuery>(),It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<IReadOnlyList<PpeProductPricing>>>(Result.Success<IReadOnlyList<PpeProductPricing>>([])));
        Assert.That((await host.Client.GetAsync(path+"?productIds=42&productIds=43")).StatusCode,Is.EqualTo(HttpStatusCode.OK));
        host.Sender.Verify(x=>x.Send(It.Is<GetPpeProductPricesQuery>(q=>q.ProductIds.SequenceEqual(new[] {42,43})),It.IsAny<CancellationToken>()),Times.Once);
        host.Sender.Setup(x=>x.Send(It.IsAny<CreatePpeOfferVersionCommand>(),It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PpeOfferVersion>>(Result.NotFound()));
        var expected=Guid.NewGuid();var request=Guid.NewGuid();
        var response=await host.Client.PostAsJsonAsync(path+"/42/offer/versions",new {expectedVersionId=expected,requestId=request,
            values=new { ruleCode="Rule",supplierCost=100,externalRetail=(decimal?)null,externalWholesale=(decimal?)null,source="source",sourceDate="2026-10-25",sourceReviewed=true},actor="spoof"});
        Assert.That(response.StatusCode,Is.EqualTo(HttpStatusCode.NotFound));
        host.Sender.Verify(x=>x.Send(It.Is<CreatePpeOfferVersionCommand>(c=>c.ProductId==42 && c.RequestId==request && c.ExpectedVersionId==expected && c.Actor=="claim-manager"),It.IsAny<CancellationToken>()),Times.Once);
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
