using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Settings;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Features.Quotes.Shared.Pdf;

namespace UnitTests.Crm.Quotes;

/// <summary>R-29: the product's main photo replaces an empty or old-site photo of a КП item.</summary>
[TestFixture]
public sealed class QuoteItemPhotosTests
{
    private const string Main = "https://brevi-media.cdn.express/library/main.jpg";

    [TestCase(null, true)]
    [TestCase(" ", true)]
    [TestCase("https://brevi.com.ua/image/catalog/costum_leto/a.jpg", true)]
    [TestCase("https://WWW.brevi.com.ua/image/a.jpg", true)]
    [TestCase("https://brevi-media.cdn.express/library/other.jpg", false)]
    public void Needs_the_catalog_photo_only_when_empty_or_from_the_old_site(string? url, bool expected) =>
        Assert.That(QuoteItemPhotos.NeedsCatalogPhoto(url), Is.EqualTo(expected));

    [Test]
    public void A_photo_picked_in_the_quote_stays()
    {
        var photos = new Dictionary<int, string> { [10] = Main };

        Assert.That(QuoteItemPhotos.Resolve(10, "https://cdn/picked.jpg", photos), Is.EqualTo("https://cdn/picked.jpg"));
        Assert.That(QuoteItemPhotos.Resolve(10, "https://brevi.com.ua/image/old.jpg", photos), Is.EqualTo(Main));
        Assert.That(QuoteItemPhotos.Resolve(10, null, photos), Is.EqualTo(Main));
        Assert.That(QuoteItemPhotos.Resolve(null, null, photos), Is.Null);
        Assert.That(QuoteItemPhotos.Resolve(11, "https://brevi.com.ua/image/old.jpg", photos),
            Is.EqualTo("https://brevi.com.ua/image/old.jpg"), "a product without a main photo keeps the old one");
    }

    [Test]
    public async Task Asks_the_catalog_only_for_items_that_need_a_photo()
    {
        var catalog = new FakeQuoteCatalog()
            .With(FakeQuoteCatalog.Model() with { PhotoUrl = Main })
            .With(FakeQuoteCatalog.Ppe() with { PhotoUrl = "https://cdn/ppe-main.jpg" });

        var photos = await QuoteItemPhotos.CatalogPhotosAsync(
            [(10, null), (20, "https://cdn/picked.jpg"), (null, null)], catalog, default);

        Assert.That(photos, Is.EqualTo(new Dictionary<int, string> { [10] = Main }));
    }

    [Test]
    public void Pdf_shows_the_main_photo_of_an_item_from_the_old_site()
    {
        var item = new QuoteItem(0, QuoteItemKind.Ppe, 10,
            new QuoteItemTexts("PPE-1", "Рукавиці", null, null, null, "https://brevi.com.ua/image/old.jpg", null, null),
            [new QuoteItemOption(0, null, null, "Ціна", null, false, new QuotePrice(100m, null, true),
                new QuotePrice(90m, null, true), new QuotePrice(80m, null, true), DateTimeOffset.UnixEpoch)]);
        var quote = Quote.Create(new QuoteNumber(2026, 11, 5), QuoteKind.Request, new QuotePerson(5, "Юлія"),
            new QuoteHeader("КП", null, null, null, null, null), [item], DateTimeOffset.UnixEpoch).Value;

        var model = QuotePdfModelBuilder.Build(quote, QuoteSettingsDefaults.Content,
            new QuoteStaffContact(5, "Юлія", null, null), new Dictionary<int, string> { [10] = Main });

        Assert.That(model.Items[0].PhotoUrl, Is.EqualTo(Main));
    }
}
