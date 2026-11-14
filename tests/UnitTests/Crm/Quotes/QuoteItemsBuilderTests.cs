using Ardalis.Result;
using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Building;

namespace UnitTests.Crm.Quotes;

public sealed class QuoteItemsBuilderTests
{
    private static readonly QuotePerson Manager = new(17, "Олена");
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    private static QuotePriceInput Shown(decimal? manual = null) => new(true, manual);
    private static QuotePriceInput Hidden() => new(false, null);

    private static QuoteItemInput Sewing(int productId = 10, long articleId = 100, long? id = null, long? optionId = null,
        bool showRetail = true) =>
        new(id, "Sewing", productId, null, null, null, null,
            [new QuoteOptionInput(optionId, articleId, true, showRetail ? Shown() : Hidden(), Shown(), Shown(), null)]);

    private static QuoteItemInput Ppe(decimal? manual = null, string? reason = null, long? id = null, long? optionId = null) =>
        new(id, "Ppe", 20, null, null, null, null,
            [new QuoteOptionInput(optionId, null, false, Shown(manual), Shown(), Shown(), reason)]);

    [Test]
    public async Task NewSewingVariantTakesPublishedPricesAndCatalogTexts()
    {
        var catalog = new FakeQuoteCatalog().With(FakeQuoteCatalog.Model(variants: FakeQuoteCatalog.Variant()));

        var result = await new QuoteItemsBuilder(catalog).BuildAsync([Sewing()], [], Manager, Now, default);

        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.ValidationErrors.Select(x => x.ErrorMessage)));
        var item = result.Value.Single();
        var option = item.Options.Single();
        Assert.Multiple(() =>
        {
            Assert.That(item.Name, Is.EqualTo("Костюм «Імпульс»"));
            Assert.That(item.Subtitle, Is.EqualTo(QuoteItemsBuilder.SewingSubtitle));
            Assert.That(option.Title, Is.EqualTo("Саржа"));
            Assert.That(option.Retail.Amount, Is.EqualTo(900m));
            Assert.That(option.Large.Amount, Is.EqualTo(800m));
            Assert.That(option.PricesTakenAt, Is.EqualTo(Now));
        });
    }

    [Test]
    public async Task SavedVariantKeepsItsPriceWhenTheCatalogChanged()
    {
        var catalog = new FakeQuoteCatalog().With(FakeQuoteCatalog.Model(variants: FakeQuoteCatalog.Variant()));
        var builder = new QuoteItemsBuilder(catalog);
        var saved = (await builder.BuildAsync([Sewing()], [], Manager, Now, default)).Value;
        catalog.With(FakeQuoteCatalog.Model(variants: FakeQuoteCatalog.Variant(retail: 1500)));

        var again = await builder.BuildAsync([Sewing(id: 0, optionId: 0, showRetail: false)], saved, Manager, Now, default);

        var option = again.Value.Single().Options.Single();
        Assert.That(option.Retail.Source, Is.EqualTo(900m));
        Assert.That(option.Retail.Shown, Is.False);
    }

    [Test]
    public async Task VariantWithoutPublishedPriceCannotBeAdded()
    {
        var catalog = new FakeQuoteCatalog().With(FakeQuoteCatalog.Model(variants: FakeQuoteCatalog.Variant(retail: null)));

        var result = await new QuoteItemsBuilder(catalog).BuildAsync([Sewing()], [], Manager, Now, default);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        Assert.That(result.Errors.Single(), Does.Contain("NO_LABOR"));
    }

    [Test]
    public async Task DisabledProductCannotBeAdded()
    {
        var catalog = new FakeQuoteCatalog().With(FakeQuoteCatalog.Model(active: false, variants: FakeQuoteCatalog.Variant()));

        var result = await new QuoteItemsBuilder(catalog).BuildAsync([Sewing()], [], Manager, Now, default);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task PpeManualPriceNeedsAReasonAndRecordsWhoSetIt()
    {
        var catalog = new FakeQuoteCatalog().With(FakeQuoteCatalog.Ppe());
        var builder = new QuoteItemsBuilder(catalog);

        var withoutReason = await builder.BuildAsync([Ppe(manual: 250)], [], Manager, Now, default);
        var withReason = await builder.BuildAsync([Ppe(manual: 250, reason: "Знижка за обсяг")], [], Manager, Now, default);

        Assert.That(withoutReason.Status, Is.EqualTo(ResultStatus.Invalid));
        var option = withReason.Value.Single().Options.Single();
        Assert.Multiple(() =>
        {
            Assert.That(option.Retail.Amount, Is.EqualTo(250m));
            Assert.That(option.Retail.Source, Is.EqualTo(300m));
            Assert.That(option.ManualByName, Is.EqualTo("Олена"));
            Assert.That(option.ManualAt, Is.EqualTo(Now));
        });
    }

    [Test]
    public async Task ManualPriceEqualToCatalogIsNotManual()
    {
        var catalog = new FakeQuoteCatalog().With(FakeQuoteCatalog.Ppe());

        var result = await new QuoteItemsBuilder(catalog).BuildAsync([Ppe(manual: 300)], [], Manager, Now, default);

        Assert.That(result.Value.Single().Options.Single().HasManualPrice, Is.False);
    }

    [Test]
    public async Task ItemThatShowsNoPositivePriceIsRejected()
    {
        var input = new QuoteItemInput(null, "OneOffPpe", null, "Каска", null, null, null,
            [new QuoteOptionInput(null, null, false, Shown(0), Hidden(), Hidden(), null)]);

        var result = await new QuoteItemsBuilder(new FakeQuoteCatalog()).BuildAsync([input], [], Manager, Now, default);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
    }

    [Test]
    public async Task OneOffPpeUsesManualPricesWithoutReason()
    {
        var input = new QuoteItemInput(null, "OneOffPpe", null, " Каска ", "Біла", null, "https://supplier.example",
            [new QuoteOptionInput(null, null, false, Shown(410), Shown(390), Hidden(), null)]);

        var result = await new QuoteItemsBuilder(new FakeQuoteCatalog()).BuildAsync([input], [], Manager, Now, default);

        var item = result.Value.Single();
        Assert.Multiple(() =>
        {
            Assert.That(item.Name, Is.EqualTo("Каска"));
            Assert.That(item.Subtitle, Is.EqualTo(QuoteItemsBuilder.OneOffSubtitle));
            Assert.That(item.Options.Single().Wholesale.Amount, Is.EqualTo(390m));
        });
    }
}
