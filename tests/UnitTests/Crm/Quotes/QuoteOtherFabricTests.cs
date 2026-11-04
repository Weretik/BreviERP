using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Validation;
using Crm.Domain.Quotes;

namespace UnitTests.Crm.Quotes;

/// <summary>A Catalog model row in another fabric from the Reference (R-27), priced from the model's shared lines.</summary>
public sealed class QuoteOtherFabricTests
{
    private static readonly QuotePerson Manager = new(17, "Олена");
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    private static QuoteOptionInput Catalog(long? id = null) =>
        new(id, 100, false, new QuotePriceInput(true, null), new QuotePriceInput(true, null),
            new QuotePriceInput(true, null), null);

    private static QuoteOptionInput Other(int fabricId, decimal meters, long? id = null) =>
        new(id, null, true, new QuotePriceInput(true, null), new QuotePriceInput(true, null),
            new QuotePriceInput(true, null), null, [new QuoteOptionFabricInput(fabricId, meters)]);

    private static QuoteItemInput Model(long? id, params QuoteOptionInput[] options) =>
        new(id, "Sewing", 10, null, null, null, null, options);

    private static QuoteItemsBuilder Builder(FakeSewingCalculator calculator) =>
        new(new FakeQuoteCatalog().With(FakeQuoteCatalog.Model(variants: FakeQuoteCatalog.Variant())), calculator,
            new FakeModelCompositionReader());

    [Test]
    public async Task Another_fabric_is_priced_with_the_model_lines_next_to_the_catalog_variant()
    {
        var calculator = new FakeSewingCalculator();

        var result = await Builder(calculator).BuildAsync([Model(null, Catalog(), Other(6, 1.4m))], [], Manager, Now,
            default);

        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        var options = result.Value.Single().Options;
        Assert.Multiple(() =>
        {
            Assert.That(options[0].Retail.Source, Is.EqualTo(900m));
            Assert.That(options[1].ArticleId, Is.Null);
            Assert.That(options[1].Title, Is.EqualTo("Тканина 6"));
            Assert.That(options[1].IsRecommended, Is.True);
            Assert.That(options[1].Fabrics.Single().Meters, Is.EqualTo(1.4m));
            Assert.That(options[1].Retail.Source, Is.EqualTo((1.4m * 300m + 50m) * 2));
            Assert.That(calculator.Calls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task A_saved_row_keeps_its_price_until_its_fabric_changes()
    {
        var calculator = new FakeSewingCalculator();
        var builder = Builder(calculator);
        var saved = (await builder.BuildAsync([Model(null, Other(6, 1.4m))], [], Manager, Now, default)).Value;
        var price = saved.Single().Options.Single().Retail.Source;
        calculator.FabricPrices[6] = 1000m;

        var same = await builder.BuildAsync([Model(0, Other(6, 1.4m, id: 0))], saved, Manager, Now, default);
        var changed = await builder.BuildAsync([Model(0, Other(6, 2m, id: 0))], saved, Manager, Now, default);

        Assert.Multiple(() =>
        {
            Assert.That(same.Value.Single().Options.Single().Retail.Source, Is.EqualTo(price));
            Assert.That(changed.Value.Single().Options.Single().Retail.Source, Is.EqualTo((2m * 1000m + 50m) * 2));
            Assert.That(calculator.Calls, Is.EqualTo(2));
        });
    }

    [Test]
    public void A_model_row_takes_one_fabric_from_the_reference()
    {
        var two = new QuoteOptionInput(null, null, false, new QuotePriceInput(true, null), new QuotePriceInput(true, null),
            new QuotePriceInput(true, null), null, [new QuoteOptionFabricInput(5, 1m), new QuoteOptionFabricInput(6, 1m)]);

        var valid = new QuoteItemInputValidator().Validate(Model(null, Other(6, 1.4m)));
        var invalid = new QuoteItemInputValidator().Validate(Model(null, two));

        Assert.Multiple(() =>
        {
            Assert.That(valid.IsValid, Is.True, string.Join(",", valid.Errors));
            Assert.That(invalid.IsValid, Is.False);
        });
    }
}
