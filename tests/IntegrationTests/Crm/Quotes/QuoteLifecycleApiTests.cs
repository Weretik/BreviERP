using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Quotes.ArchiveQuote;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.GetQuoteVersions;
using Crm.Application.Quotes.GetQuoteAssignees;
using Crm.Application.Quotes.Ownership;
using Crm.Application.Quotes.RepriceQuote;
using Crm.Application.Quotes.TransferQuote;
using IntegrationTests.Crm.Support;
using Moq;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteLifecycleApiTests
{
    [TestCase("GET", "versions")]
    [TestCase("GET", "events")]
    [TestCase("GET", "versions/11111111-1111-1111-1111-111111111111")]
    [TestCase("POST", "reprice")]
    [TestCase("POST", "copy")]
    [TestCase("POST", "transfer")]
    [TestCase("PUT", "archive")]
    public async Task LifecycleEndpointsDenyAnonymousAndUser(string method, string suffix)
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var path = $"/api/crm/quotes/{Guid.NewGuid()}/{suffix}";
        using var first = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(new { }) };
        Assert.That((await host.Client.SendAsync(first)).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using var actor = CrmApiTestHost.AsRole(host.Client, "User", "17");
        using var second = new HttpRequestMessage(new HttpMethod(method), path) { Content = JsonContent.Create(new { }) };
        Assert.That((await host.Client.SendAsync(second)).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task VersionHistoryPreservesEnvelopePagingAndAuthenticatedActor()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var quoteId = Guid.NewGuid();
        host.Sender.Setup(x => x.Send(It.IsAny<GetQuoteVersionsQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PagedResult<IReadOnlyList<QuoteVersionSummary>>>>(
                Result.Success(new PagedResult<IReadOnlyList<QuoteVersionSummary>>(new PagedInfo(2, 10, 3, 21), []))));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var response = await host.Client.GetAsync($"/api/crm/quotes/{quoteId}/versions?page=2&pageSize=10");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt32(), Is.EqualTo(21));
        Assert.That(json.GetProperty("value").GetArrayLength(), Is.Zero);
        host.Sender.Verify(x => x.Send(It.Is<GetQuoteVersionsQuery>(q =>
            q.Actor.UserId == 17 && !q.Actor.IsAdmin && q.QuoteId == quoteId && q.Page == 2 && q.PageSize == 10),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ConfirmationConflictAndTransferAccessLossUseTruthfulStatuses()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<RepriceQuoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuoteDetail>>(Result.Conflict("source_changed")));
        host.Sender.Setup(x => x.Send(It.IsAny<TransferQuoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuoteDetail>>(Result.NotFound()));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var quoteId = Guid.NewGuid();
        var reprice = await host.Client.PostAsJsonAsync($"/api/crm/quotes/{quoteId}/reprice", new
        {
            requestId = Guid.NewGuid(),
            expectedRevision = 1,
            versionId = Guid.NewGuid(),
            bases = new[] { new { variantId = Guid.NewGuid(), basisToken = Guid.NewGuid() } }
        });
        Assert.That(reprice.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        var transfer = await host.Client.PostAsJsonAsync($"/api/crm/quotes/{quoteId}/transfer", new
        { requestId = Guid.NewGuid(), expectedRevision = 1, ownerId = 18, reason = "Transfer" });
        Assert.That(transfer.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ForgedActorAndTotalsAreRejectedBeforeLifecycleHandlers()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var quoteId = Guid.NewGuid();
        var forged = await host.Client.PostAsJsonAsync($"/api/crm/quotes/{quoteId}/reprice", new
        { requestId = Guid.NewGuid(), expectedRevision = 1, versionId = Guid.NewGuid(), bases = Array.Empty<object>(), total = 1 });
        Assert.That(forged.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var archive = await host.Client.PutAsJsonAsync($"/api/crm/quotes/{quoteId}/archive", new
        { requestId = Guid.NewGuid(), expectedRevision = 1, archived = true, reason = "Done", actorId = 18 });
        Assert.That(archive.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<RepriceQuoteCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        host.Sender.Verify(x => x.Send(It.IsAny<ArchiveQuoteCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AssigneeSearchIsRoleProtectedAndUsesAuthenticatedActor()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        Assert.That((await host.Client.GetAsync("/api/crm/quote-assignees")).StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        using (CrmApiTestHost.AsRole(host.Client, "User", "17"))
            Assert.That((await host.Client.GetAsync("/api/crm/quote-assignees")).StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        host.Sender.Setup(x => x.Send(It.IsAny<GetQuoteAssigneesQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<PagedResult<IReadOnlyList<QuoteAssignee>>>>(
                Result.Success(new PagedResult<IReadOnlyList<QuoteAssignee>>(new PagedInfo(1, 25, 1, 1),
                    [new(18, "Synthetic manager")]))));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var response = await host.Client.GetAsync("/api/crm/quote-assignees?search=Synthetic");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.GetProperty("value")[0].EnumerateObject().Select(x => x.Name),
            Is.EquivalentTo(new[] { "userId", "displayName" }));
        host.Sender.Verify(x => x.Send(It.Is<GetQuoteAssigneesQuery>(q =>
            q.Actor.UserId == 17 && q.Search == "Synthetic"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task MissingArchiveDecisionIsRejectedBeforeHandler()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<ArchiveQuoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuoteDetail>>(Result.Conflict("unexpected_dispatch")));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var response = await host.Client.PutAsJsonAsync($"/api/crm/quotes/{Guid.NewGuid()}/archive", new
        { requestId = Guid.NewGuid(), expectedRevision = 1, reason = "Decision must be explicit" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<ArchiveQuoteCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task ExplicitArchiveDecisionReachesHandler(bool archived)
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<ArchiveQuoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuoteDetail>>(Result.Conflict("revision_conflict")));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var response = await host.Client.PutAsJsonAsync($"/api/crm/quotes/{Guid.NewGuid()}/archive", new
        { requestId = Guid.NewGuid(), expectedRevision = 1, archived, reason = "Explicit decision" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        host.Sender.Verify(x => x.Send(It.Is<ArchiveQuoteCommand>(command => command.Archived == archived),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
