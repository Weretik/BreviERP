using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.GetQuotes;
using Crm.Application.Quotes.Pricing;
using Crm.Application.Quotes.PreviewQuotePrices;
using Crm.Application.Quotes.SaveQuoteVersion;
using IntegrationTests.Crm.Support;
using Moq;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteApiTests
{
    [Test]
    public async Task AnonymousAndUserCannotReadOrPreview()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        Assert.That((await host.Client.GetAsync("/api/crm/quotes")).StatusCode,
            Is.EqualTo(HttpStatusCode.Unauthorized));
        using var actor = CrmApiTestHost.AsRole(host.Client, "User", "22");
        Assert.That((await host.Client.GetAsync("/api/crm/quotes")).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That((await host.Client.PostAsJsonAsync("/api/crm/quotes/price-previews", new { })).StatusCode,
            Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task ListPreservesPagingAndActorScope()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        host.Sender.Setup(x => x.Send(It.IsAny<GetQuotesQuery>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<PagedResult<IReadOnlyList<QuoteSummary>>>(
                new PagedResult<IReadOnlyList<QuoteSummary>>(new PagedInfo(2, 25, 3, 51), [])));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "22");
        var response = await host.Client.GetAsync("/api/crm/quotes?page=2");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(json.GetProperty("pagedInfo").GetProperty("totalRecords").GetInt64(), Is.EqualTo(51));
        Assert.That((await host.Client.GetAsync("/api/crm/quotes?page=0")).StatusCode,
            Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.Is<GetQuotesQuery>(q =>
            q.Actor.UserId == 22 && !q.Actor.IsAdmin && q.Page == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ForgedTotalsAreRejectedBeforePreviewHandler()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "22");
        var response = await host.Client.PostAsJsonAsync("/api/crm/quotes/price-previews", new
        {
            requestId = Guid.NewGuid(),
            intent = new
            {
                kind = "sewing",
                productId = 25,
                articleId = 246,
                total = 1
            }
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        host.Sender.Verify(x => x.Send(It.IsAny<PreviewQuotePricesCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task PreviewAndCreateUseAuthenticatedActor()
    {
        await using var host = await CrmApiTestHost.CreateAsync();
        var intent = new QuotePriceIntent("sewing", 25, 246, null, null);
        var now = DateTimeOffset.UtcNow;
        var preview = new QuotePricePreview(Guid.NewGuid(), now.AddMinutes(15), intent,
            null, 1169.74m, 996.17m, 943.34m, [], "UAH", "catalog_no_extra_vat");
        host.Sender.Setup(x => x.Send(It.IsAny<PreviewQuotePricesCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuotePricePreview>>(Result.Success(preview)));
        var quoteId = Guid.NewGuid();
        var detail = new QuoteDetail(quoteId, null, null, "Synthetic", 1, "draft", false,
            null, 22, now, now, true,
            new QuoteVersionView(Guid.NewGuid(), 1, "created", "Synthetic", null, null,
                "UAH", "no_extra_vat", 22, now, []));
        host.Sender.Setup(x => x.Send(It.IsAny<CreateQuoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Result<QuoteDetail>>(Result.Success(detail)));
        using var actor = CrmApiTestHost.AsRole(host.Client, "Manager", "22");
        var price = await host.Client.PostAsJsonAsync("/api/crm/quotes/price-previews", new
        {
            requestId = Guid.NewGuid(),
            intent
        });
        Assert.That(price.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var create = await host.Client.PostAsJsonAsync("/api/crm/quotes", new
        {
            requestId = Guid.NewGuid(),
            recipientName = "Synthetic",
            content = new { items = Array.Empty<object>() }
        });
        Assert.That(create.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(create.Headers.Location!.AbsolutePath, Is.EqualTo($"/api/crm/quotes/{quoteId}"));
        host.Sender.Verify(x => x.Send(It.Is<PreviewQuotePricesCommand>(q => q.Actor.UserId == 22),
            It.IsAny<CancellationToken>()), Times.Once);
        host.Sender.Verify(x => x.Send(It.Is<CreateQuoteCommand>(q => q.Actor.UserId == 22),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
