using Catalog.Api.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Ardalis.Result;
using Catalog.Api;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Identity.Api.DependencyInjection;
using Catalog.Application.Features.Pricing.Commands.ImportPpeSupplierFiles;
using Catalog.Application.Features.Pricing.Commands.RefreshPpeSupplierPrices;
using Catalog.Application.Features.Pricing.Commands.UpdatePpePriceSource;
using Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;

namespace IntegrationTests.Catalog.Pricing.Ppe.SupplierPrices;

/// <summary>«Імпорт цін» HTTP access: read for staff, discounts for admins, imports and «Оновити зараз» for managers (CAT-017).</summary>
[NonParallelizable]
public sealed class PpeSupplierPricesApiTests
{
    private const string Path = "/api/v1/pricing/ppe/supplier-prices";

    [Test]
    public async Task Only_admins_change_discounts_and_the_actor_comes_from_the_session()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<UpdatePpePriceSourceCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result>(Result.Success()));
        host.Sender.Setup(x => x.Send(It.IsAny<GetPpePriceSourcesQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<IReadOnlyList<PpePriceSourceView>>>(Result.Success<IReadOnlyList<PpePriceSourceView>>([])));

        Assert.That((await host.Client.GetAsync(Path)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        host.As("Manager", "manager-id", "manager@brevi");
        var managerView = await host.Client.GetFromJsonAsync<System.Text.Json.JsonElement>(Path);
        Assert.That(managerView.GetProperty("canChangeDiscounts").GetBoolean(), Is.False);
        Assert.That((await host.Client.PutAsJsonAsync(Path + "/av-center", new { discountPercent = 15 })).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));

        host.As("Admin", "admin-id", "admin@brevi");
        var adminView = await host.Client.GetFromJsonAsync<System.Text.Json.JsonElement>(Path);
        Assert.That(adminView.GetProperty("canChangeDiscounts").GetBoolean(), Is.True);
        var response = await host.Client.PutAsJsonAsync(Path + "/av-center", new { discountPercent = 15, supplierId = 4 });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        host.Sender.Verify(x => x.Send(It.Is<UpdatePpePriceSourceCommand>(c =>
            c.Key == "av-center" && c.DiscountPercent == 15 && c.SupplierId == 4 && c.Actor == "admin@brevi"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Import_passes_every_uploaded_file_and_previews_by_default()
    {
        await using var host = await TestHost.CreateAsync();
        ImportPpeSupplierFilesCommand? sent = null;
        string? firstContent = null;
        host.Sender.Setup(x => x.Send(It.IsAny<ImportPpeSupplierFilesCommand>(), It.IsAny<CancellationToken>()))
            .Callback<ICommand<Result<PpePriceImportResult>>, CancellationToken>((request, _) =>
            {
                var command = (ImportPpeSupplierFilesCommand)request;
                sent = command;
                firstContent = new StreamReader(command.Files[0].Content).ReadToEnd();
            })
            .Returns(new ValueTask<Result<PpePriceImportResult>>(
                Result.Success(new PpePriceImportResult("insight", false, 0, 0, 0, 0, 0, []))));

        host.As("User", "viewer", "viewer@brevi");
        Assert.That((await host.Client.PostAsync(Path + "/insight/import", Files("a.xml"))).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        host.As("Manager", "manager-id", "manager@brevi");
        var response = await host.Client.PostAsync(Path + "/insight/import", Files("a.xml", "b.xml", "c.xml"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.Multiple(() =>
        {
            Assert.That(sent!.DryRun, Is.True);
            Assert.That(sent.Files.Select(x => x.Name), Is.EqualTo(new[] { "a.xml", "b.xml", "c.xml" }));
            Assert.That(firstContent, Is.EqualTo("content of a.xml"));
            Assert.That((sent.Actor, sent.ActorName), Is.EqualTo(("manager-id", "manager@brevi")));
        });

        await host.Client.PostAsync(Path + "/insight/import?dryRun=false", Files("a.xml", "b.xml", "c.xml"));
        Assert.That(sent!.DryRun, Is.False);
    }

    [Test]
    public async Task Refresh_previews_by_default_and_applies_with_dry_run_false()
    {
        await using var host = await TestHost.CreateAsync();
        RefreshPpeSupplierPricesCommand? sent = null;
        host.Sender.Setup(x => x.Send(It.IsAny<RefreshPpeSupplierPricesCommand>(), It.IsAny<CancellationToken>()))
            .Callback<ICommand<Result<PpePriceImportResult>>, CancellationToken>((request, _) => sent = (RefreshPpeSupplierPricesCommand)request)
            .Returns(new ValueTask<Result<PpePriceImportResult>>(
                Result.Success(new PpePriceImportResult("av-center", false, 20, 0, 0, 0, 0, []))));

        host.As("User", "viewer", "viewer@brevi");
        Assert.That((await host.Client.PostAsync(Path + "/av-center/refresh", null)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        host.As("Manager", "manager-id", "manager@brevi");
        Assert.That((await host.Client.PostAsync(Path + "/av-center/refresh", null)).StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((sent!.Key, sent.DryRun, sent.Actor, sent.ActorName), Is.EqualTo(("av-center", true, "manager-id", "manager@brevi")));

        await host.Client.PostAsync(Path + "/av-center/refresh?dryRun=false", null);
        Assert.That(sent.DryRun, Is.False);
    }

    private static MultipartFormDataContent Files(params string[] names)
    {
        var form = new MultipartFormDataContent();
        foreach (var name in names)
        {
            var file = new StringContent("content of " + name);
            file.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
            form.Add(file, "files", name);
        }

        return form;
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
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, role.ToString()),
                new(ClaimTypes.NameIdentifier, Request.Headers["X-Test-UserId"].ToString()),
                new(ClaimTypes.Name, Request.Headers["X-Test-UserName"].ToString())
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }

    private sealed class TestHost(WebApplication app, HttpClient client, Mock<ISender> sender) : IAsyncDisposable
    {
        public HttpClient Client { get; } = client;
        public Mock<ISender> Sender { get; } = sender;

        public void As(string role, string userId, string userName)
        {
            foreach (var header in new[] { "X-Test-Role", "X-Test-UserId", "X-Test-UserName" })
                Client.DefaultRequestHeaders.Remove(header);
            Client.DefaultRequestHeaders.Add("X-Test-Role", role);
            Client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
            Client.DefaultRequestHeaders.Add("X-Test-UserName", userName);
        }

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
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
            });
            // «Права доступу»: `access:` policies with the catalogue defaults.
            builder.Services.AddAccessPolicies();
            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapCatalogEndpoints();
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
