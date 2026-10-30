using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.EnqueueQuote;
using Crm.Application.Delivery.GetDeliveries;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.Readiness;
using IntegrationTests.Crm.Support;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegrationTests.Crm.Delivery;

public sealed class QuoteDeliveryApiTests
{
    [TestCase("POST", "")]
    [TestCase("GET", "")]
    [TestCase("GET", "/11111111-1111-1111-1111-111111111111")]
    public async Task All_delivery_routes_require_staff_role(string method, string suffix)
    {
        await using var host = await CreateAsync(new Mock<IQuoteDeliveryStore>(MockBehavior.Strict));
        var path = $"/api/crm/quotes/{Guid.NewGuid()}/deliveries{suffix}";
        using var anonymous = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(Request()) };
        Assert.That((await host.Client.SendAsync(anonymous)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using var role = CrmApiTestHost.AsRole(host.Client, "User", "17");
        using var denied = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(Request()) };
        Assert.That((await host.Client.SendAsync(denied)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Queue_rejects_unmapped_body_before_store_and_invalid_actor_claims()
    {
        var store = new Mock<IQuoteDeliveryStore>(MockBehavior.Strict);
        await using var host = await CreateAsync(store);
        var path = $"/api/crm/quotes/{Guid.NewGuid()}/deliveries";
        using (CrmApiTestHost.AsRole(host.Client, "Manager", "17"))
        {
            var json = JsonSerializer.Serialize(Request());
            using var content = new StringContent(json[..^1] + ",\"subject\":\"forged\"}", System.Text.Encoding.UTF8, "application/json");
            Assert.That((await host.Client.PostAsync(path, content)).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That((await host.Client.PostAsJsonAsync(path, new { })).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        using (CrmApiTestHost.AsRole(host.Client, "Manager", "invalid"))
            Assert.That((await host.Client.PostAsJsonAsync(path, Request())).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        store.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Explicit_enqueue_returns202_and_safe_metadata_with_authenticated_actor()
    {
        var store = new Mock<IQuoteDeliveryStore>(MockBehavior.Strict);
        var detail = Detail();
        store.Setup(x => x.EnqueueAsync(It.IsAny<EnqueueQuoteDeliveryCommand>(), It.IsAny<CrmActor>(),
            It.IsAny<IQuoteDeliveryIdentity>(), It.IsAny<IQuoteDeliveryReadiness>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(detail));
        await using var host = await CreateAsync(store);
        using var role = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var response = await host.Client.PostAsJsonAsync($"/api/crm/quotes/{detail.QuoteId}/deliveries", Request());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(body.GetProperty("status").GetString(), Is.EqualTo("queued"));
        Assert.That(body.TryGetProperty("pdfBytes", out _), Is.False);
        Assert.That(body.TryGetProperty("plainText", out _), Is.False);
        store.Verify(x => x.EnqueueAsync(It.Is<EnqueueQuoteDeliveryCommand>(q => q.QuoteId == detail.QuoteId),
            It.Is<CrmActor>(a => a.UserId == 17 && !a.IsAdmin), It.IsAny<IQuoteDeliveryIdentity>(),
            It.IsAny<IQuoteDeliveryReadiness>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase(ResultStatus.Unavailable, HttpStatusCode.ServiceUnavailable)]
    [TestCase(ResultStatus.NotFound, HttpStatusCode.NotFound)]
    [TestCase(ResultStatus.Conflict, HttpStatusCode.Conflict)]
    public async Task Queue_preserves_expected_readiness_scope_and_conflict_status(ResultStatus status, HttpStatusCode expected)
    {
        var outcome = status switch {
            ResultStatus.Unavailable => Result<QuoteDeliveryDetail>.Unavailable("delivery_not_ready"),
            ResultStatus.NotFound => Result<QuoteDeliveryDetail>.NotFound(),
            _ => Result<QuoteDeliveryDetail>.Conflict("revision_conflict") };
        var store = new Mock<IQuoteDeliveryStore>(MockBehavior.Strict);
        store.Setup(x => x.EnqueueAsync(It.IsAny<EnqueueQuoteDeliveryCommand>(), It.IsAny<CrmActor>(),
            It.IsAny<IQuoteDeliveryIdentity>(), It.IsAny<IQuoteDeliveryReadiness>(), It.IsAny<CancellationToken>())).ReturnsAsync(outcome);
        await using var host = await CreateAsync(store);
        using var role = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        Assert.That((await host.Client.PostAsJsonAsync($"/api/crm/quotes/{Guid.NewGuid()}/deliveries", Request())).StatusCode, Is.EqualTo(expected));
    }

    [Test]
    public async Task History_returns_paged_envelope_and_current_safe_detail()
    {
        var detail = Detail();
        var store = new Mock<IQuoteDeliveryStore>(MockBehavior.Strict);
        store.Setup(x => x.ListAsync(detail.QuoteId, It.IsAny<CrmActor>(), 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PagedResult<IReadOnlyList<QuoteDeliveryDetail>>(new PagedInfo(2, 10, 2, 11), new[] { detail })));
        store.Setup(x => x.GetAsync(detail.QuoteId, detail.DeliveryId, It.IsAny<CrmActor>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(detail));
        await using var host = await CreateAsync(store);
        using var role = CrmApiTestHost.AsRole(host.Client, "Admin", "7");
        var page = await host.Client.GetFromJsonAsync<JsonElement>($"/api/crm/quotes/{detail.QuoteId}/deliveries?page=2&pageSize=10");
        Assert.That(page.GetProperty("value").GetArrayLength(), Is.EqualTo(1));
        Assert.That(page.TryGetProperty("pagedInfo", out _), Is.True);
        var item = await host.Client.GetFromJsonAsync<JsonElement>($"/api/crm/quotes/{detail.QuoteId}/deliveries/{detail.DeliveryId}");
        Assert.That(item.GetProperty("deliveryId").GetGuid(), Is.EqualTo(detail.DeliveryId));
    }

    private static Task<CrmApiTestHost> CreateAsync(Mock<IQuoteDeliveryStore> store) => CrmApiTestHost.CreateAsync(services =>
    {
        services.AddSingleton(store.Object);
        services.AddSingleton(Mock.Of<IQuoteDeliveryIdentity>());
        services.AddSingleton(Mock.Of<IQuoteDeliveryReadiness>());
        services.AddScoped<EnqueueQuoteDeliveryHandler>();
        services.AddScoped<QuoteDeliveryReader>();
    });
    private static object Request() => new { requestId = Guid.NewGuid(), expectedRevision = 2, versionId = Guid.NewGuid(),
        artifactId = Guid.NewGuid(), emailTemplateVersionId = Guid.NewGuid(), recipientAddress = "sandbox@example.test", managerName = "Manager" };
    private static QuoteDeliveryDetail Detail() => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        "sandbox@example.test", "queued", 0, null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);
}
