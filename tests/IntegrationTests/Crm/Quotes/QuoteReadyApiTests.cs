using System.Net;
using System.Net.Http.Json;
using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.PrepareQuote;
using IntegrationTests.Crm.Support;
using Moq;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteReadyApiTests
{
    [Test]
    public async Task Ready_route_keeps_staff_access_and_authenticated_actor_after_controller_split()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var quoteId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var path = $"/api/crm/quotes/{quoteId}/ready";
        var body = new { versionId, expectedRevision = 3, requestId };

        Assert.That((await host.Client.PostAsJsonAsync(path, body)).StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized));
        using (CrmApiTestHost.AsRole(host.Client, "User", "17"))
            Assert.That((await host.Client.PostAsJsonAsync(path, body)).StatusCode,
                Is.EqualTo(HttpStatusCode.Forbidden));

        host.Sender.Setup(x => x.Send(It.IsAny<PrepareQuoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuoteDetail>>(Result.Conflict("revision_conflict")));
        using (CrmApiTestHost.AsRole(host.Client, "Manager", "17"))
            Assert.That((await host.Client.PostAsJsonAsync(path, body)).StatusCode,
                Is.EqualTo(HttpStatusCode.Conflict));

        host.Sender.Verify(x => x.Send(It.Is<PrepareQuoteCommand>(command =>
            command.QuoteId == quoteId && command.VersionId == versionId &&
            command.ExpectedRevision == 3 && command.RequestId == requestId &&
            command.Actor.UserId == 17 && !command.Actor.IsAdmin),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
