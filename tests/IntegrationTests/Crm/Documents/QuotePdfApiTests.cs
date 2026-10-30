using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.GetQuoteArtifact;
using Crm.Application.Documents.RenderQuotePdf;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;
using IntegrationTests.Crm.Support;
using Moq;

namespace IntegrationTests.Crm.Documents;

[NonParallelizable]
public sealed class QuotePdfApiTests
{
    [TestCase("POST", "")]
    [TestCase("GET", "/11111111-1111-1111-1111-111111111111")]
    [TestCase("GET", "/11111111-1111-1111-1111-111111111111/content")]
    public async Task Pdf_routes_require_staff_role(string method, string suffix)
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var path = $"/api/crm/quotes/{Guid.NewGuid()}/documents{suffix}";
        using var anonymous = new HttpRequestMessage(new HttpMethod(method), path)
        { Content = JsonContent.Create(new { versionId = Guid.NewGuid() }) };
        Assert.That((await host.Client.SendAsync(anonymous)).StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized));
        using var role = CrmApiTestHost.AsRole(host.Client, "User", "17");
        using var unauthorized = new HttpRequestMessage(new HttpMethod(method), path)
        { Content = JsonContent.Create(new { versionId = Guid.NewGuid() }) };
        Assert.That((await host.Client.SendAsync(unauthorized)).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Render_rejects_unmapped_input_before_handler()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var role = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var response = await host.Client.PostAsJsonAsync($"/api/crm/quotes/{Guid.NewGuid()}/documents",
            new { versionId = Guid.NewGuid(), internalCost = 1 });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<RenderQuotePdfCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Render_and_download_use_authenticated_actor_and_public_metadata_only()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var quoteId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var artifactId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var detail = new QuotePdfArtifactDetail(artifactId, "queued", null, null, null,
            now, now, null);
        host.Sender.Setup(x => x.Send(It.IsAny<RenderQuotePdfCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuotePdfArtifactDetail>>(Result.Success(detail)));
        var metadata = new QuotePdfArtifactMetadata(artifactId, new QuoteId(5), 7,
            new IssuerSettingsVersionId(8), new DocumentTemplateVersionId(9), "standard-v1",
            "succeeded", 1, "AABB", 8, null, now, now, now);
        var bytes = Encoding.ASCII.GetBytes("%PDF-1.7");
        host.Sender.Setup(x => x.Send(It.IsAny<GetQuoteArtifactContentQuery>(),
                It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuotePdfArtifactContent>>(
                Result.Success(new QuotePdfArtifactContent(metadata, bytes))));
        using var role = CrmApiTestHost.AsRole(host.Client, "Manager", "17");
        var render = await host.Client.PostAsJsonAsync($"/api/crm/quotes/{quoteId}/documents",
            new { versionId });
        Assert.That(render.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        var response = await render.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(response.EnumerateObject().Select(x => x.Name), Is.EquivalentTo(new[]
            { "artifactId", "status", "sha256", "length", "errorCode", "createdAt", "updatedAt", "generatedAt" }));
        var download = await host.Client.GetAsync($"/api/crm/quotes/{quoteId}/documents/{artifactId}/content");
        Assert.Multiple(() =>
        {
            Assert.That(download.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(download.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/pdf"));
        });
        Assert.That(await download.Content.ReadAsByteArrayAsync(), Is.EqualTo(bytes));
        host.Sender.Verify(x => x.Send(It.Is<RenderQuotePdfCommand>(c =>
            c.QuoteId == quoteId && c.VersionId == versionId && c.Actor.UserId == 17),
            It.IsAny<CancellationToken>()), Times.Once);
        host.Sender.Verify(x => x.Send(It.Is<GetQuoteArtifactContentQuery>(q =>
            q.QuoteId == quoteId && q.ArtifactId == artifactId && q.Actor.UserId == 17),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
