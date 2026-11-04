using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.CustomSewing;
using Crm.Application.Quotes.Validation;
using Crm.Domain.Quotes;

namespace UnitTests.Crm.Quotes;

/// <summary>«Виріб з нуля» (R-20): priced by the Catalog formula, kept on save while its composition stays.</summary>
public sealed class QuoteCustomSewingTests
{
    private static readonly QuotePerson Manager = new(17, "Олена");
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    private static QuoteOptionInput Fabric(int fabricId, decimal meters, long? id = null, bool recommended = false) =>
        new(id, null, recommended, new QuotePriceInput(true, null), new QuotePriceInput(true, null),
            new QuotePriceInput(true, null), null, fabricId, meters);

    private static QuoteItemInput Custom(long? id = null, decimal operationQuantity = 2m, params QuoteOptionInput[] fabrics) =>
        new(id, "CustomSewing", null, "Фартух з логотипом", "Під замовлення", null, null,
            fabrics.Length == 0 ? [Fabric(5, 1.2m)] : fabrics,
            [
                new QuoteItemLineInput("Accessory", 30, 4m, null),
                new QuoteItemLineInput("Operation", 40, operationQuantity, null)
            ]);

    [Test]
    public async Task A_new_item_is_priced_per_fabric_and_keeps_its_composition()
    {
        var calculator = new FakeSewingCalculator();
        var builder = new QuoteItemsBuilder(new FakeQuoteCatalog(), calculator);

        var result = await builder.BuildAsync([Custom(fabrics: [Fabric(5, 1.2m), Fabric(6, 1m, recommended: true)])],
            [], Manager, Now, default);

        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.ValidationErrors.Select(x => x.ErrorMessage)));
        var item = result.Value.Single();
        Assert.Multiple(() =>
        {
            Assert.That(item.Kind, Is.EqualTo(QuoteItemKind.CustomSewing));
            Assert.That(item.ProductId, Is.Null);
            Assert.That(item.Subtitle, Is.EqualTo(QuoteCustomSewingItemBuilder.Subtitle));
            Assert.That(item.Lines.Select(x => (x.Kind, x.Name, x.Quantity)), Is.EqualTo(new[]
            {
                (QuoteItemLineKind.Accessory, "Accessory 30", 4m), (QuoteItemLineKind.Operation, "Operation 40", 2m)
            }));
            Assert.That(item.Options.Select(x => x.Title), Is.EqualTo(new[] { "Тканина 5", "Тканина 6" }));
            Assert.That(item.Options[0].FabricMeters, Is.EqualTo(1.2m));
            Assert.That(item.Options[0].Retail.Source, Is.EqualTo((1.2m * 200m + 100m) * 2));
            Assert.That(item.Options[1].IsRecommended, Is.True);
            Assert.That(calculator.Calls, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task A_saved_item_keeps_its_prices_until_the_composition_changes()
    {
        var calculator = new FakeSewingCalculator();
        var builder = new QuoteItemsBuilder(new FakeQuoteCatalog(), calculator);
        var saved = (await builder.BuildAsync([Custom()], [], Manager, Now, default)).Value;
        var price = saved.Single().Options.Single().Retail.Source;
        calculator.FabricPrices[5] = 999m;

        var same = await builder.BuildAsync([Custom(id: 0, fabrics: Fabric(5, 1.2m, id: 0))], saved, Manager, Now, default);
        var changed = await builder.BuildAsync([Custom(id: 0, operationQuantity: 3m, fabrics: Fabric(5, 1.2m, id: 0))],
            saved, Manager, Now, default);

        Assert.Multiple(() =>
        {
            Assert.That(same.Value.Single().Options.Single().Retail.Source, Is.EqualTo(price));
            Assert.That(changed.Value.Single().Options.Single().Retail.Source, Is.EqualTo((1.2m * 999m + 150m) * 2));
            Assert.That(calculator.Calls, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task A_fabric_without_a_price_or_a_missing_line_stops_the_save()
    {
        var builder = new QuoteItemsBuilder(new FakeQuoteCatalog(), new FakeSewingCalculator());
        var gone = Custom() with { Lines = [new QuoteItemLineInput("Operation", 404, 1m, null)] };

        var noPrice = await builder.BuildAsync([Custom(fabrics: Fabric(7, 1m))], [], Manager, Now, default);
        var missing = await builder.BuildAsync([gone], [], Manager, Now, default);

        Assert.Multiple(() =>
        {
            Assert.That(noPrice.Status, Is.EqualTo(Ardalis.Result.ResultStatus.Conflict));
            Assert.That(noPrice.Errors.Single(), Does.Contain("немає ціни: тканина"));
            Assert.That(missing.Errors.Single(), Does.Contain("операцію не знайдено"));
        });
    }

    [Test]
    public void The_validator_needs_a_name_an_operation_and_a_fabric_with_metres()
    {
        var input = new QuoteItemInput(null, "CustomSewing", null, " ", null, null, null,
            [Fabric(0, 0m)], [new QuoteItemLineInput("Accessory", 30, 1m, null)]);

        var errors = new QuoteItemInputValidator().Validate(input).Errors.Select(x => x.ErrorMessage).ToArray();

        Assert.That(errors, Is.SupersetOf(new[]
        {
            "Вкажіть назву виробу з нуля.", "Додайте до виробу з нуля хоча б одну операцію.",
            "Оберіть тканину і вкажіть її метраж."
        }));
    }

    [Test]
    public void Other_items_cannot_carry_a_composition()
    {
        var input = new QuoteItemInput(null, "Sewing", 10, null, null, null, null,
            [new QuoteOptionInput(null, 100, false, new(true, null), new(true, null), new(true, null), null)],
            [new QuoteItemLineInput("Operation", 40, 1m, null)]);

        var errors = new QuoteItemInputValidator().Validate(input).Errors.Select(x => x.ErrorMessage);

        Assert.That(errors, Does.Contain("Склад задається лише для виробу з нуля."));
    }
}
