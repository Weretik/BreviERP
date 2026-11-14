using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Settings;
using Crm.Application.Features.Embroidery.Commands.SaveEmbroiderySettings;
using Crm.Application.Features.Embroidery.Queries.CalculateEmbroidery;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Features.Embroidery.Shared.Access;
using Crm.Application.Features.Embroidery.Shared.Mapping;
using Crm.Application.Abstractions.Embroidery;

namespace UnitTests.Crm.Embroidery;

[TestFixture]
public sealed class EmbroideryContractTests
{
    private static SaveEmbroiderySettingsInput DefaultInput()
    {
        var response = ((EmbroiderySettings?)null).ToResponse(new EmbroideryStitchRate(1m, null));
        return new SaveEmbroiderySettingsInput(response.Version, response.Rates, response.Markups, response.Stabilizer,
            response.Fabric, response.FabricIncreasePercent, response.Hoops, response.Expenses);
    }

    private static bool Valid(SaveEmbroiderySettingsInput input) => new SaveEmbroiderySettingsValidator()
        .Validate(new SaveEmbroiderySettingsCommand(new EmbroideryActor(1, true), input)).IsValid;

    private static CalculateEmbroideryInput Calculation() =>
        new("Garment", 50, 10_000, "18", 1, 1, 1, "New", 60, null, "UsedBlanks", null);

    private static bool Valid(CalculateEmbroideryInput input) => new CalculateEmbroideryValidator()
        .Validate(new CalculateEmbroideryQuery(new EmbroideryActor(1, false), input)).IsValid;

    [Test]
    public void DefaultsRoundTripAsValidSettings()
    {
        var input = DefaultInput();

        Assert.That(Valid(input), Is.True);
        Assert.That(input.Version, Is.Zero);
        Assert.That(input.Markups.Select(x => x.Name), Is.EqualTo(new[] { "Категорія 1", "Категорія 2" }));
        Assert.That(input.ToContent(), Is.Not.Null);
        Assert.That(EmbroiderySettingsRules.Check(input.ToContent()).IsSuccess, Is.True);
    }

    [Test]
    public void SettingsRejectBrokenValues()
    {
        var input = DefaultInput();

        Assert.That(Valid(input with { Markups = input.Markups.Take(1).ToArray() }), Is.False);
        Assert.That(Valid(input with { Markups = [.. input.Markups, input.Markups[0]] }), Is.False);
        Assert.That(Valid(input with { Stabilizer = input.Stabilizer with { WidthCm = 0 } }), Is.False);
        Assert.That(Valid(input with { Hoops = [] }), Is.False);
        Assert.That(Valid(input with { Hoops = [.. input.Hoops, input.Hoops[0]] }), Is.False);
        Assert.That(Valid(input with { Expenses = [input.Expenses[0] with { Basis = "1" }] }), Is.False);
        Assert.That(Valid(input with { Expenses = [input.Expenses[0] with { Rate = -1 }] }), Is.False);
        Assert.That(Valid(input with { Rates = input.Rates with { TaxPercent = -5 } }), Is.False);
    }

    [Test]
    public void EmptyRateStaysEmptyAndZeroStaysZero()
    {
        var input = DefaultInput();
        var content = (input with
        {
            Expenses = [input.Expenses[0] with { Rate = null }, input.Expenses[1] with { Rate = 0m }],
        }).ToContent();

        Assert.That(content.Expenses.Select(x => x.Rate), Is.EqualTo(new decimal?[] { null, 0m }));
        Assert.That(content.ToForm(false, null).Expenses.Select(x => x.HasRate), Is.EqualTo(new[] { false, true }));
    }

    [Test]
    public void CalculationInputNeedsKnownCodesAndPositiveNumbers()
    {
        Assert.That(Valid(Calculation()), Is.True);
        Assert.That(Valid(Calculation() with { Mode = "chevron" }), Is.False);
        Assert.That(Valid(Calculation() with { Quantity = 0 }), Is.False);
        Assert.That(Valid(Calculation() with { StitchesPerUnit = -1 }), Is.False);
        Assert.That(Valid(Calculation() with { MachineMinutes = -1 }), Is.False);
        Assert.That(Valid(Calculation() with { Mode = "Chevron", UnitsPerHoop = 0 }), Is.False);
        Assert.That(Valid(Calculation() with { UnitsPerHoop = 0 }), Is.True, "Garment embroidery ignores units per hoop.");
    }

    [Test]
    public void ManagerGetsPricesWithoutCostRatesOrMarkups()
    {
        var result = EmbroideryCalculator.Calculate(Calculation().ToDomain(), EmbroiderySettingsDefaults.Content, 1m).Result!;

        var manager = result.ToResponse(seesCost: false);
        var director = result.ToResponse(seesCost: true);

        Assert.That(manager.Cost, Is.Null);
        Assert.That(manager.Prices.All(x => x.MarkupPercent is null), Is.True);
        Assert.That(manager.Stabilizer!.Charged, Is.Null);
        Assert.That(manager.Prices.Select(x => x.Unit), Is.EqualTo(director.Prices.Select(x => x.Unit)));
        Assert.That(director.Cost!.Total, Is.EqualTo(2055m));
        Assert.That(director.Prices.Select(x => x.Name), Is.EqualTo(new[] { "Категорія 1", "Категорія 2" }));
        Assert.That(director.Prices[0].Unit, Is.EqualTo(96.83m), "Категорія 1 is the old «Стандарт», +90 %.");
    }
}
