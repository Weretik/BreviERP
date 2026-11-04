using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Composition;
using Catalog.Domain.Pricing.Sewing;
using Moq;

namespace UnitTests;

/// <summary>A free composition (КП «виріб з нуля») is priced by the Catalog formula without a Catalog article.</summary>
public sealed class SewingCompositionPricingTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    private static readonly SewingCalculationSettings Settings =
        new(24_000m, 20m, 480m, 5m, 0.1m, 0.1m, 0.05m, 0.05m, 0.05m, 48_000m, 2m, 1.8m, 1.5m);

    private static CalculateSewingCompositionQueryHandler Handler(params PricingReferenceValue[] values)
    {
        var settings = new Mock<ISewingPricingSettingsReader>();
        settings.Setup(x => x.ReadCandidatesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SewingPricingSettingsSelection(1, 1, Now.AddDays(-1), SewingArticleCalculation.FormulaVersion,
                Settings)]);
        var references = new Mock<IPricingReferenceVersionReader>();
        references.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<string> keys, DateTimeOffset _, CancellationToken _) =>
                values.Where(x => keys.Contains(x.ReferenceKey)).ToArray());
        return new CalculateSewingCompositionQueryHandler(settings.Object, references.Object);
    }

    private static PricingReferenceValue Value(string key, decimal value, string unit) =>
        new(key, 1, value, unit, true, true, key);

    private static CalculateSewingCompositionQuery Query(params SewingCompositionOperation[] operations) =>
        new([
                new SewingCompositionMaterial(SewingCompositionMaterialKind.Fabric, 4, 1.5m),
                new SewingCompositionMaterial(SewingCompositionMaterialKind.Accessory, 30, 4m)
            ],
            operations.Length == 0 ? [new SewingCompositionOperation(40, 2m)] : operations, Now);

    [Test]
    public async Task Fabric_accessories_and_operations_give_three_prices()
    {
        var handler = Handler(Value("Fabric:4", 200m, "м"), Value("Accessory:30", 2m, "шт."),
            Value("Operation:40", 10m, "Minutes"));

        var result = (await handler.Handle(Query(), CancellationToken.None)).Value;

        Assert.That(result.Calculation.IsCalculated, Is.True,
            string.Join(", ", result.Calculation.Blockers.Select(x => x.Code)));
        var breakdown = result.Calculation.Breakdown!;
        Assert.Multiple(() =>
        {
            Assert.That(breakdown.Materials, Is.EqualTo(308m));
            Assert.That(breakdown.TotalMinutes, Is.EqualTo(20m));
            Assert.That(result.Calculation.Prices!.Retail,
                Is.EqualTo(decimal.Round(breakdown.FullCost * 2m, 2, MidpointRounding.AwayFromZero)));
            Assert.That(result.References.Select(x => x.ReferenceKey),
                Is.EquivalentTo(new[] { "Fabric:4", "Accessory:30", "Operation:40" }));
        });
    }

    [Test]
    public async Task Manual_minutes_replace_a_missing_norm()
    {
        var handler = Handler(Value("Fabric:4", 200m, "м"), Value("Accessory:30", 2m, "шт."),
            new PricingReferenceValue("Operation:99", 1, null, "Minutes", true, true, "OP-155"));

        var result = (await handler.Handle(Query(new SewingCompositionOperation(99, 1m, 12m)), CancellationToken.None))
            .Value;

        Assert.That(result.Calculation.IsCalculated, Is.True);
        Assert.That(result.Calculation.Breakdown!.TotalMinutes, Is.EqualTo(12m));
    }

    [Test]
    public async Task A_line_without_a_price_or_norm_blocks_the_price_and_names_its_position()
    {
        var handler = Handler(Value("Fabric:4", 200m, "м"));

        var result = (await handler.Handle(Query(), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.Calculation.Prices, Is.Null);
            Assert.That(result.Calculation.Blockers.Select(x => (x.Code, x.InputKey)), Does.Contain(("MissingMaterialCost", "Material:2")));
            Assert.That(result.Calculation.Blockers.Select(x => (x.Code, x.InputKey)), Does.Contain(("MissingOperationTime", "Operation:1")));
        });
    }

    [Test]
    public void Quantities_must_be_positive()
    {
        var invalid = new CalculateSewingCompositionQuery(
            [new SewingCompositionMaterial(SewingCompositionMaterialKind.Fabric, 4, 0m)],
            [new SewingCompositionOperation(40, 1m, -1m)], Now);

        var result = new CalculateSewingCompositionQueryValidator().Validate(invalid);

        Assert.That(result.Errors.Select(x => x.PropertyName),
            Is.SupersetOf(new[] { "Materials[0].Quantity", "Operations[0].ManualMinutes" }));
    }
}
