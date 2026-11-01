using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Api.Delivery.Resolution;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.ResolveOutcome;
using IntegrationTests.Crm.Support;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegrationTests.Crm.Delivery;

public sealed class QuoteDeliveryResolutionApiTests
{
    private static readonly Guid QuoteId = Guid.NewGuid(), DeliveryId = Guid.NewGuid();
    private static readonly string Path = $"/api/crm/quotes/{QuoteId}/deliveries/{DeliveryId}/resolution";
    private static object Request() => new { requestId = Guid.NewGuid(), expectedAttempt = 1, outcome = "sent", evidenceNote = "Checked exact recipient and message." };
    private static Task<CrmApiTestHost> Host(Mock<IQuoteDeliveryStore> store) => CrmApiTestHost.CreateAsync(s => {
        s.AddSingleton(store.Object); s.AddScoped<ResolveQuoteDeliveryHandler>();
    });

    [Test]
    public async Task Resolution_requires_staff_and_never_accepts_client_actor_or_time()
    {
        await using var host = await Host(new Mock<IQuoteDeliveryStore>(MockBehavior.Strict));
        Assert.That((await host.Client.PostAsJsonAsync(Path, Request())).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using (CrmApiTestHost.AsRole(host.Client, "User", "17"))
            Assert.That((await host.Client.PostAsJsonAsync(Path, Request())).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        using (CrmApiTestHost.AsRole(host.Client, "Manager", "17"))
        {
            var raw = JsonSerializer.Serialize(Request());
            using var body = new StringContent(raw[..^1] + ",\"actorId\":1}", Encoding.UTF8, "application/json");
            Assert.That((await host.Client.PostAsync(Path, body)).StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
    }

    [TestCase(ResultStatus.Conflict, HttpStatusCode.Conflict)]
    [TestCase(ResultStatus.NotFound, HttpStatusCode.NotFound)]
    public async Task Resolution_preserves_conflict_and_scope_errors(ResultStatus status, HttpStatusCode expected)
    {
        var store = new Mock<IQuoteDeliveryStore>(MockBehavior.Strict);
        store.Setup(x => x.ResolveAsync(It.IsAny<ResolveQuoteDeliveryCommand>(), It.Is<CrmActor>(a => a.UserId == 17), It.IsAny<CancellationToken>()))
            .ReturnsAsync(status == ResultStatus.Conflict ? Result<QuoteDeliveryDetail>.Conflict("attempt_conflict") : Result<QuoteDeliveryDetail>.NotFound());
        await using var host = await Host(store);
        using var role = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        Assert.That((await host.Client.PostAsJsonAsync(Path, Request())).StatusCode, Is.EqualTo(expected));
    }

    [Test]
    public async Task Resolution_returns_manual_provenance_separately_from_provider_confirmation()
    {
        var at = DateTimeOffset.UtcNow;
        var detail = new QuoteDeliveryDetail(DeliveryId, QuoteId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "recipient@example.test", "succeeded", 1, "delivery_manually_confirmed_sent", at, at, at, false,
            new("sent", 17, 1, "Checked manually", at));
        var store = new Mock<IQuoteDeliveryStore>(MockBehavior.Strict);
        store.Setup(x => x.ResolveAsync(It.Is<ResolveQuoteDeliveryCommand>(x => x.QuoteId == QuoteId && x.DeliveryId == DeliveryId),
            It.IsAny<CrmActor>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(detail));
        await using var host = await Host(store);
        using var role = CrmApiTestHost.AsRole(host.Client, "Admin", "17");
        using var response = await host.Client.PostAsJsonAsync(Path, Request());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.GetProperty("manualResolution").GetProperty("actorId").GetInt32(), Is.EqualTo(17));
        Assert.That(json.TryGetProperty("providerMessageId", out _), Is.False);
    }

    [TestCase("maybe", "Checked manually")]
    [TestCase("sent", " ")]
    public async Task Invalid_decision_or_missing_evidence_is_rejected_before_persistence(string outcome, string evidence)
    {
        await using var host = await Host(new Mock<IQuoteDeliveryStore>(MockBehavior.Strict));
        using var role = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        using var response = await host.Client.PostAsJsonAsync(Path,
            new { requestId = Guid.NewGuid(), expectedAttempt = 1, outcome, evidenceNote = evidence });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [TestCase("{}")]
    [TestCase("{\"requestId\":\"11111111-1111-4111-8111-111111111111\",\"expectedAttempt\":1,\"outcome\":\"sent\",\"evidenceNote\":\"Checked\",\"createdAt\":\"2026-01-01T00:00:00Z\"}")]
    public void Missing_or_forged_resolution_evidence_is_not_bound(string json)
        => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ResolveQuoteDeliveryRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
}
