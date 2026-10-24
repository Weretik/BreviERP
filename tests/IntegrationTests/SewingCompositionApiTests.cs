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

namespace IntegrationTests;

[NonParallelizable]
public sealed class SewingCompositionApiTests
{
    private const string BasePath = "/api/v1/products/42";

    [Test]
    public async Task Missing_concurrency_token_is_rejected_before_mediator()
    {
        await using var host = await TestHost.CreateAsync();
        using var manager = AsRole(host.Client, "Manager", "manager-1");
        var response = await host.Client.PostAsJsonAsync($"{BasePath}/articles",
            new { saleSku = "246", active = true, sortOrder = 0 });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<CreateSewingArticleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Requires_authentication_and_product_management_policy()
    {
        await using var host = await TestHost.CreateAsync();
        var anonymous = await host.Client.GetAsync($"{BasePath}/articles");
        using var reader = AsRole(host.Client, "User", "user-1");
        var forbidden = await host.Client.PostAsJsonAsync($"{BasePath}/articles", NewArticle());

        Assert.Multiple(() =>
        {
            Assert.That(anonymous.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(forbidden.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        });
        host.Sender.Verify(x => x.Send(It.IsAny<CreateSewingArticleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Routes_reads_and_writes_and_takes_actor_from_authenticated_claims()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<ListSewingArticlesQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingArticleList>>(Result.Success(ArticleList())));
        host.Sender.Setup(x => x.Send(It.IsAny<GetSewingCompositionQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingComposition>>(Result.Success(Composition())));
        host.Sender.Setup(x => x.Send(It.IsAny<CreateSewingArticleCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingArticleView>>(Result.Success(new SewingArticleView(501, 42, "0246", true, 0))));
        host.Sender.Setup(x => x.Send(It.IsAny<ReplaceSewingCompositionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingComposition>>(Result.Success(Composition())));
        host.Sender.Setup(x => x.Send(It.IsAny<ReplaceSewingArticleCompositionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingComposition>>(Result.Success(Composition(501))));
        using var manager = AsRole(host.Client, "Manager", "auth-actor-42");

        var articles = await host.Client.GetAsync($"{BasePath}/articles");
        var composition = await host.Client.GetAsync($"{BasePath}/composition");
        var created = await host.Client.PostAsJsonAsync($"{BasePath}/articles", NewArticle());
        var replaced = await host.Client.PutAsJsonAsync($"{BasePath}/composition", NewModelComposition());
        var articleReplaced = await host.Client.PutAsJsonAsync($"{BasePath}/articles/501/composition", NewArticleComposition());
        var body = await created.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Multiple(() =>
        {
            Assert.That(articles.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(composition.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(created.Headers.Location!.AbsolutePath, Is.EqualTo($"{BasePath}/articles"));
            Assert.That(body.GetProperty("saleSku").GetString(), Is.EqualTo("0246"));
            Assert.That(replaced.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(articleReplaced.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
        host.Sender.Verify(x => x.Send(It.Is<GetSewingCompositionQuery>(q => q.ProductId == 42 && q.ArticleId == null), It.IsAny<CancellationToken>()), Times.Once);
        host.Sender.Verify(x => x.Send(It.Is<ReplaceSewingCompositionCommand>(q => q.ProductId == 42 && q.Actor == "auth-actor-42"), It.IsAny<CancellationToken>()), Times.Once);
        host.Sender.Verify(x => x.Send(It.Is<ReplaceSewingArticleCompositionCommand>(q => q.ProductId == 42 && q.ArticleId == 501 && q.Actor == "auth-actor-42"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Maps_invalid_not_found_and_conflict_results_to_contract_statuses()
    {
        await using var host = await TestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<ListSewingArticlesQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingArticleList>>(Result.NotFound()));
        host.Sender.Setup(x => x.Send(It.IsAny<CreateSewingArticleCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingArticleView>>(Result.Conflict("Sale SKU is already in use.")));
        host.Sender.Setup(x => x.Send(It.IsAny<ReplaceSewingCompositionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingComposition>>(Result<SewingComposition>.Invalid(new ValidationError("materials[0].quantity", "Quantity must be positive."))));
        host.Sender.Setup(x => x.Send(It.IsAny<ReplaceSewingArticleCompositionCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<SewingComposition>>(Result.Conflict("Composition version is stale.")));
        using var manager = AsRole(host.Client, "Admin", "actor-1");

        var missing = await host.Client.GetAsync($"{BasePath}/articles");
        var conflict = await host.Client.PostAsJsonAsync($"{BasePath}/articles", NewArticle());
        var invalid = await host.Client.PutAsJsonAsync($"{BasePath}/composition", NewModelComposition());
        var stale = await host.Client.PutAsJsonAsync($"{BasePath}/articles/501/composition", NewArticleComposition());
        var invalidBody = await invalid.Content.ReadFromJsonAsync<JsonElement>();
        var conflictBody = await conflict.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Multiple(() =>
        {
            Assert.That(missing.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(conflict.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
            Assert.That(conflictBody.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(invalid.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(invalidBody.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(invalidBody[0].GetProperty("identifier").GetString(), Is.EqualTo("materials[0].quantity"));
            Assert.That(stale.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        });
    }

    [Test]
    public async Task Write_without_name_identifier_claim_is_unauthorized()
    {
        await using var host = await TestHost.CreateAsync();
        using var managerWithoutActor = AsRole(host.Client, "Manager", null);

        var response = await host.Client.PutAsJsonAsync($"{BasePath}/composition", NewModelComposition());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        host.Sender.Verify(x => x.Send(It.IsAny<ReplaceSewingCompositionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static IDisposable AsRole(HttpClient client, string role, string? userId)
    {
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        if (userId is not null) client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        return new HeaderRemoval(client);
    }

    private static CreateSewingArticleRequest NewArticle() => new("0246", true, 0, 0);
    private static ReplaceSewingCompositionRequest NewModelComposition() => new("MOD-025", [], [], 0);
    private static ReplaceSewingArticleCompositionRequest NewArticleComposition() => new([], 0);
    private static SewingArticleList ArticleList() => new(42, "MOD-025", 0, [new(501, 42, "0246", true, 0)]);
    private static SewingComposition Composition(long? articleId = null) => new(42, articleId, "MOD-025", 0, [], [], []);

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
            builder.Services.AddControllers().AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly);

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
