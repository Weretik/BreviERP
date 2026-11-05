using Purchasing.Application.Contracts;
using Purchasing.Application.Register.GetInvoices;

namespace UnitTests.Purchasing;

public sealed class PurchaseInvoiceQueryTests
{
    private PurchasingTestHarness harness = null!;

    [SetUp]
    public void SetUp() => harness = new PurchasingTestHarness();

    [TearDown]
    public void TearDown() => harness.Dispose();

    private async Task<PurchaseInvoicePage> ListAsync(string? search = null, PurchaseInvoiceFilter filter = PurchaseInvoiceFilter.All)
    {
        var result = await new GetPurchaseInvoicesQueryHandler(harness.Db)
            .Handle(new GetPurchaseInvoicesQuery(1, 50, search, filter), CancellationToken.None);
        Assert.That(result.IsSuccess, Is.True);
        return result.Value;
    }

    [Test]
    public async Task OneCardPerMessageWithTheFirstRowAsRepresentativeNewestFirst()
    {
        harness.Sheet.Rows =
        [
            SheetRow.Of("100", messageId: "1", dateTime: "01.11.2026 09:00"),
            SheetRow.Of("200", messageId: "2", dateTime: "04.11.2026 09:00", fileLinks: "https://www.dropbox.com/b.pdf"),
            SheetRow.Of("201", messageId: "2", dateTime: "04.11.2026 09:00", fileLinks: "https://www.dropbox.com/a.pdf"),
            SheetRow.Of("300", messageId: "3", dateTime: "колись"),
        ];
        await harness.Sync().RunAsync(CancellationToken.None);

        var page = await ListAsync();

        Assert.That(page.Total, Is.EqualTo(3));
        Assert.That(page.Items.Select(x => x.SourceUid), Is.EqualTo(new[] { "200", "100", "300" }));
        var card = page.Items[0];
        Assert.That(card.SourceRowCount, Is.EqualTo(2));
        Assert.That(card.RowNumber, Is.EqualTo(3));
        Assert.That(card.Links.Select(x => x.Url), Is.EqualTo(new[] { "https://www.dropbox.com/a.pdf", "https://www.dropbox.com/b.pdf" }));
        Assert.That(card.Links.All(x => x.Safe), Is.True);
        Assert.That((card.ChatName, card.FromUser), Is.EqualTo(("БРЕВІ команда", "Олена")));
        Assert.That(card.State, Is.EqualTo(PurchaseInvoiceState.AwaitingRecognition));
    }

    [Test]
    public async Task NeedsReviewFilterAndSearchByTextOrUid()
    {
        harness.Sheet.Rows =
        [
            SheetRow.Of("100", messageId: "1", text: "Фурнітура YKK"),
            SheetRow.Of("200", messageId: "2", text: "Тканина", fileLinks: "https://drive.google.com/x"),
        ];
        await harness.Sync().RunAsync(CancellationToken.None);

        var review = await ListAsync(filter: PurchaseInvoiceFilter.NeedsReview);
        Assert.That(review.Items.Select(x => x.SourceUid), Is.EqualTo(new[] { "200" }));
        Assert.That(review.Items[0].State, Is.EqualTo(PurchaseInvoiceState.NeedsReview));
        Assert.That(review.Items[0].Links.Single().Safe, Is.False);

        Assert.That((await ListAsync("ykk")).Items.Select(x => x.SourceUid), Is.EqualTo(new[] { "100" }));
        Assert.That((await ListAsync("200")).Items.Select(x => x.SourceUid), Is.EqualTo(new[] { "200" }));
    }
}
