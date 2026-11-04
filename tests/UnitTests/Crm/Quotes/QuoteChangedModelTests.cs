using Crm.Application.Quotes.Building;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.CustomSewing;
using Crm.Application.Quotes.Validation;
using Crm.Domain.Quotes;

namespace UnitTests.Crm.Quotes;

/// <summary>
/// «Змінити склад» (R-21): a Catalog model with a changed composition is a «Виріб з нуля» linked to its model; it
/// keeps the model's code, link and season and carries services and extra fabrics.
/// </summary>
public sealed class QuoteChangedModelTests
{
    private static readonly QuotePerson Manager = new(17, "Олена");
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    private static QuoteOptionInput Fabric(int fabricId, decimal meters, long? id = null) =>
        new(id, null, false, new QuotePriceInput(true, null), new QuotePriceInput(true, null),
            new QuotePriceInput(true, null), null, fabricId, meters);

    private static QuoteItemInput Changed(long? id = null, string service = "Вишивка логотипу", int productId = 10) =>
        new(id, "CustomSewing", productId, "Костюм «Імпульс» без кишень", "Змінений склад", null, null,
            [Fabric(5, 1.4m, id is null ? null : 0)],
            [
                new QuoteItemLineInput("Accessory", 30, 2m, null),
                new QuoteItemLineInput("Fabric", 8, 0.3m, null),
                new QuoteItemLineInput("Service", 0, 1m, null, service),
                new QuoteItemLineInput("Operation", 40, 1m, null)
            ]);

    private static QuoteItemsBuilder Builder(FakeSewingCalculator? calculator = null) =>
        new(new FakeQuoteCatalog().With(FakeQuoteCatalog.Model(variants: FakeQuoteCatalog.Variant())),
            calculator ?? new FakeSewingCalculator());

    [Test]
    public async Task A_changed_model_keeps_its_model_and_carries_services_and_extra_fabrics()
    {
        var result = await Builder().BuildAsync([Changed()], [], Manager, Now, default);

        Assert.That(result.IsSuccess, Is.True, string.Join(",", result.Errors));
        var item = result.Value.Single();
        Assert.Multiple(() =>
        {
            Assert.That(item.Kind, Is.EqualTo(QuoteItemKind.CustomSewing));
            Assert.That(item.ProductId, Is.EqualTo(10));
            Assert.That(item.Code, Is.EqualTo("MOD-1"));
            Assert.That(item.SiteUrl, Is.EqualTo("https://brevi.com.ua/impuls"));
            Assert.That(item.Season, Is.EqualTo("Літо"));
            Assert.That(item.Subtitle, Is.EqualTo(QuoteCustomSewingItemBuilder.ChangedModelSubtitle));
            Assert.That(item.Name, Is.EqualTo("Костюм «Імпульс» без кишень"));
            Assert.That(item.Lines.Select(x => (x.Kind, x.ReferenceId, x.CostCode)), Is.EqualTo(new[]
            {
                (QuoteItemLineKind.Accessory, 30, (string?)null), (QuoteItemLineKind.Fabric, 8, null),
                (QuoteItemLineKind.Service, 0, "Вишивка логотипу"), (QuoteItemLineKind.Operation, 40, null)
            }));
        });
    }

    [Test]
    public async Task A_changed_service_prices_the_item_again()
    {
        var calculator = new FakeSewingCalculator();
        var builder = Builder(calculator);
        var saved = (await builder.BuildAsync([Changed()], [], Manager, Now, default)).Value;

        await builder.BuildAsync([Changed(id: 0)], saved, Manager, Now, default);
        Assert.That(calculator.Calls, Is.EqualTo(1));
        await builder.BuildAsync([Changed(id: 0, service: "Друк")], saved, Manager, Now, default);
        Assert.That(calculator.Calls, Is.EqualTo(2));
    }

    [Test]
    public async Task A_model_missing_from_the_Catalog_cannot_be_changed()
    {
        var result = await Builder().BuildAsync([Changed(productId: 99)], [], Manager, Now, default);

        Assert.That(result.Errors.Single(), Does.Contain("модель не знайдено в каталозі"));
    }

    [Test]
    public void A_service_needs_its_code_and_only_a_service_has_one()
    {
        var input = Changed() with
        {
            Lines =
            [
                new QuoteItemLineInput("Service", 0, 1m, null),
                new QuoteItemLineInput("Accessory", 30, 1m, null, "X"),
                new QuoteItemLineInput("Fabric", 8, 1m, 5m),
                new QuoteItemLineInput("Operation", 40, 1m, null)
            ]
        };

        var errors = new QuoteItemInputValidator().Validate(input).Errors.Select(x => x.ErrorMessage).ToArray();

        Assert.That(errors, Is.SupersetOf(new[]
        {
            "Вкажіть код послуги.", "Код задається лише для послуги.", "Хвилини операції мають бути більше нуля."
        }).Or.SupersetOf(new[] { "Вкажіть код послуги.", "Код задається лише для послуги.", "Хвилини задаються лише для операції." }));
    }

    [Test]
    public async Task The_model_composition_is_read_for_the_panel_or_not_found()
    {
        var reader = new FakeModelCompositionReader();
        var handler = new GetQuoteModelCompositionQueryHandler(reader);

        var found = await handler.Handle(new GetQuoteModelCompositionQuery(10), default);
        var missing = await handler.Handle(new GetQuoteModelCompositionQuery(11), default);

        Assert.Multiple(() =>
        {
            Assert.That(found.Value.Fabrics.Single().Meters, Is.EqualTo(1.4m));
            Assert.That(found.Value.Lines.Select(x => (x.Kind, x.CostCode)), Is.EqualTo(new[]
            {
                ("Service", (string?)"Вишивка"), ("Operation", null)
            }));
            Assert.That(missing.Status, Is.EqualTo(Ardalis.Result.ResultStatus.NotFound));
        });
    }

    private sealed class FakeModelCompositionReader : IQuoteModelCompositionReader
    {
        public Task<QuoteModelComposition?> ReadAsync(int productId, CancellationToken cancellationToken) =>
            Task.FromResult(productId != 10
                ? null
                : new QuoteModelComposition(10, [new QuoteModelFabric(100, 5, "Саржа", 1.4m, true)],
                [
                    new QuoteModelLine(QuoteItemLineKind.Service, 0, "Вишивка", "Вишивка", "послуга", 1m, null),
                    new QuoteModelLine(QuoteItemLineKind.Operation, 40, null, "Пошив", "хв", 1m, null)
                ]));
    }
}
