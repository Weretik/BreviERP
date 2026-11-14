using Catalog.Domain.Pricing.Sewing;
using Moq;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Queries.CalculateSewingComposition;

namespace UnitTests;

/// <summary>A free composition (КП «виріб з нуля») is priced by the Catalog formula without a Catalog article.</summary>
public sealed class SewingCompositionPricingTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    private static readonly SewingCalculationSettings Settings =
        new(24_000m, 20m, 480m, 5m, 0.1m, 0.1m, 0.05m, 0.05m, 0.05m, 48_000m, 2m, 1.8m, 1.5m);

    private static CalculateSewingCompositionHandler Handler(params PricingReferenceValue[] values)
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
        return new CalculateSewingCompositionHandler(settings.Object, references.Object);
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
    public async Task A_service_and_an_extra_fabric_of_a_changed_model_are_priced_too()
    {
        var handler = Handler(Value("Fabric:4", 200m, "м"), Value("Fabric:8", 50m, "м"),
            Value("OtherCost:Вишивка", 25m, "Service"), Value("Operation:40", 10m, "Minutes"));
        var query = new CalculateSewingCompositionQuery([
                new SewingCompositionMaterial(SewingCompositionMaterialKind.Fabric, 4, 1.5m),
                new SewingCompositionMaterial(SewingCompositionMaterialKind.Fabric, 8, 0.5m),
                new SewingCompositionMaterial(SewingCompositionMaterialKind.OtherCost, 0, 2m, "Вишивка")
            ],
            [new SewingCompositionOperation(40, 2m)], Now);

        var result = (await handler.Handle(query, CancellationToken.None)).Value;

        Assert.That(result.Calculation.IsCalculated, Is.True,
            string.Join(", ", result.Calculation.Blockers.Select(x => x.Code)));
        Assert.Multiple(() =>
        {
            Assert.That(result.Calculation.Breakdown!.Materials, Is.EqualTo(1.5m * 200m + 0.5m * 50m));
            Assert.That(result.Calculation.Breakdown!.OtherCosts, Is.EqualTo(2m * 25m));
        });
    }

    [Test]
    public void A_service_needs_its_code_and_other_lines_a_reference()
    {
        var query = new CalculateSewingCompositionQuery([
                new SewingCompositionMaterial(SewingCompositionMaterialKind.OtherCost, 0, 1m),
                new SewingCompositionMaterial(SewingCompositionMaterialKind.Accessory, 0, 1m, "X")
            ],
            [new SewingCompositionOperation(40, 1m)], Now);

        var errors = new CalculateSewingCompositionValidator().Validate(query).Errors
            .Select(x => x.PropertyName).ToArray();

        Assert.That(errors, Is.SupersetOf(new[]
        {
            "Materials[0].CostCode", "Materials[1].ReferenceId", "Materials[1].CostCode"
        }));
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

        var result = new CalculateSewingCompositionValidator().Validate(invalid);

        Assert.That(result.Errors.Select(x => x.PropertyName),
            Is.SupersetOf(new[] { "Materials[0].Quantity", "Operations[0].ManualMinutes" }));
    }
}
