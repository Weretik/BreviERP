using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Settings;

namespace UnitTests.Crm.Embroidery;

/// <summary>Control examples of the old calculator's specification v217 (sections 15–17).</summary>
[TestFixture]
public sealed class EmbroideryCalculatorTests
{
    /// <summary>The specification's test settings: the original defaults with every extra cost set to an explicit 0.</summary>
    private static EmbroiderySettingsContent ZeroExtras(EmbroiderySettingsContent? source = null)
    {
        var content = source ?? EmbroiderySettingsDefaults.Content;
        return content with
        {
            Markups = [new("Опт", 60m), new("Стандарт", 90m), new("Терміново", 120m)],
            Stabilizer = new EmbroideryMaterial(20m, 90m),
            Fabric = new EmbroideryMaterial(80m, 150m),
            FabricIncreasePercent = 40m,
            Rates = new EmbroideryRates(0.30m, 10m, 300m, 24m),
            Hoops = [.. content.Hoops.Select(x => x with { GlueRate = null })],
            Expenses = [.. content.Expenses.Select(x => x with { Rate = 0m })],
        };
    }

    private static EmbroideryInput Input(EmbroideryMode mode = EmbroideryMode.Garment, string hoop = "18", int perHoop = 1,
        EmbroideryProgram program = EmbroideryProgram.New, EmbroideryMaterialMode material = EmbroideryMaterialMode.UsedBlanks,
        int quantity = 50, int stitches = 10_000, decimal? machineMinutes = 0, IReadOnlyDictionary<string, bool>? overrides = null,
        int stabilizerLayers = 1) =>
        new(mode, quantity, stitches, hoop, perHoop, stabilizerLayers, 1, program, 60, machineMinutes, material,
            overrides ?? new Dictionary<string, bool>());

    private static EmbroideryResult Price(EmbroideryInput input, EmbroiderySettingsContent? settings = null)
    {
        var calculation = EmbroideryCalculator.Calculate(input, settings ?? ZeroExtras(), 1m);
        Assert.That(calculation.Error, Is.Null);
        return calculation.Result!;
    }

    [Test]
    public void ExampleA_GarmentEmbroidery()
    {
        var result = Price(Input());

        Assert.That(result.Cost, Is.EqualTo(1505m));
        Assert.That(result.Hoopings, Is.EqualTo(50));
        Assert.That(result.Stabilizer, Is.EqualTo(new EmbroideryCut(22m, 22m, 90m, 50, 4, 13, 52, 2, 2m, 2.86m, 1.10m, 57.20m, 55m)));
        Assert.That(result.Fabric, Is.Null);
        Assert.That(result.Rows.Take(5).Select(x => (x.Amount, x.PerUnit)),
            Is.EqualTo(new[] { (500m, 10m), (150m, 3m), (500m, 10m), (55m, 1.10m), (300m, 6m) }));
        Assert.That(result.Prices.Select(x => (x.Unit, x.Total, x.RepeatUnit)), Is.EqualTo(new[]
        {
            (59.72m, 2986.00m, 47.81m), (70.92m, 3546.00m, 56.78m), (82.11m, 4105.50m, 65.74m),
        }));
        Assert.That(result.Incomplete, Is.False);
    }

    [Test]
    public void FullCutReplacesTheUsedBlanksCost()
    {
        var result = Price(Input(material: EmbroideryMaterialMode.FullCut));

        Assert.That(result.Rows.Single(x => x.Kind == EmbroideryCostKind.Stabilizer).Amount, Is.EqualTo(57.20m));
        Assert.That(result.Cost, Is.EqualTo(1507.20m));
    }

    [Test]
    public void ExampleB_Chevrons()
    {
        var result = Price(Input(EmbroideryMode.Chevron, "32", perHoop: 4));

        Assert.That((result.Hoopings, result.FullHoopings, result.LastHoopUnits, result.FreeInLastHoop), Is.EqualTo((13, 12, 2, 2)));
        Assert.That(result.Stabilizer, Is.EqualTo(new EmbroideryCut(40m, 40m, 90m, 13, 2, 7, 14, 1, 10m, 2.80m, 4m, 56m, 52m)));
        Assert.That(result.Fabric, Is.EqualTo(new EmbroideryCut(56m, 56m, 150m, 13, 2, 7, 14, 1, 38m, 3.92m, 22.40m, 313.60m, 291.20m)));
        Assert.That(result.Cost, Is.EqualTo(1423.20m));
        Assert.That(result.Prices.Select(x => (x.Unit, x.Total)),
            Is.EqualTo(new[] { (56.47m, 2823.50m), (67.06m, 3353.00m), (77.65m, 3882.50m) }));
    }

    [Test]
    public void ExampleB_ReadyProgramLeavesOutOnlyDevelopment()
    {
        var result = Price(Input(EmbroideryMode.Chevron, "32", perHoop: 4, program: EmbroideryProgram.Ready));

        Assert.That(result.Cost, Is.EqualTo(1123.20m));
        Assert.That(result.Prices.Select(x => (x.Unit, x.Total)),
            Is.EqualTo(new[] { (44.57m, 2228.50m), (52.93m, 2646.50m), (61.28m, 3064.00m) }));
    }

    [Test]
    public void ExampleB_FullCutChangesBothMaterials()
    {
        var result = Price(Input(EmbroideryMode.Chevron, "32", perHoop: 4, material: EmbroideryMaterialMode.FullCut));

        Assert.That(result.Cost, Is.EqualTo(1449.60m));
        Assert.That(result.Prices.Select(x => x.Unit), Is.EqualTo(new[] { 57.52m, 68.31m, 79.09m }));
    }

    [Test]
    public void AdjustmentCostsTheSameAsNew() =>
        Assert.That(Price(Input(program: EmbroideryProgram.Adjustment)).Cost, Is.EqualTo(1505m));

    [Test]
    public void BlankCostIsNotRoundedBeforeMultiplying()
    {
        // Hoop 15: fabric blank 25.2 × 25.2, five across a 150 cm roll; one blank costs exactly 4.032.
        var result = Price(Input(EmbroideryMode.Chevron, "15", perHoop: 1, quantity: 125));

        Assert.That(result.Fabric!.Across, Is.EqualTo(5));
        Assert.That(result.Fabric.Charged, Is.EqualTo(504m));
    }

    [Test]
    public void LayersAreAddedBeforeRowsAreRounded()
    {
        // 3 hoopings × 2 layers = 6 blanks, 4 across: 2 rows, not 1 + 1 rounded per layer.
        var result = Price(Input(quantity: 3, stabilizerLayers: 2));

        Assert.That((result.Stabilizer.Blanks, result.Stabilizer.Rows), Is.EqualTo((6, 2)));
    }

    [Test]
    public void HalfAKopeckRoundsUp()
    {
        var settings = ZeroExtras() with
        {
            Rates = new EmbroideryRates(0m, 0m, 0m, 0m),
            Markups = [new("Опт", 0m), new("Стандарт", 0m), new("Терміново", 0m)],
            Stabilizer = new EmbroideryMaterial(0m, 90m),
            Expenses = [new EmbroideryExpense("order", "Разові", EmbroideryExpenseBasis.Order, 0.005m, true, EmbroideryExpenseMode.All)],
        };

        var result = EmbroideryCalculator.Calculate(Input(quantity: 1, stitches: 1), settings, 0m).Result!;

        Assert.That(result.Prices[0].Unit, Is.EqualTo(0.01m));
    }

    [Test]
    public void EmptyRateMakesThePriceIncompleteButKeepsTheOtherCosts()
    {
        var settings = ZeroExtras() with
        {
            Expenses = [.. ZeroExtras().Expenses.Select(x => x.Id == "preparation" ? x with { Rate = null } : x)],
        };

        var result = Price(Input(), settings);

        Assert.That(result.Missing, Is.EqualTo(new[] { "Підготовка партії та пробник" }));
        Assert.That(result.Rows.Any(x => x.Name == "Підготовка партії та пробник"), Is.False);
        Assert.That(result.Cost, Is.EqualTo(1505m));
    }

    [Test]
    public void ExplicitZeroIsARowAndUntickedIsNothing()
    {
        var result = Price(Input(overrides: new Dictionary<string, bool> { ["orderOther"] = false }));

        Assert.That(result.Rows.Single(x => x.Name == "Підготовка партії та пробник").Amount, Is.Zero);
        Assert.That(result.Rows.Any(x => x.Name == "Інші разові витрати"), Is.False);
        Assert.That(result.Missing, Is.Empty);
    }

    [Test]
    public void ChevronOnlyCostsAreLeftOutOfGarmentEmbroidery()
    {
        var result = Price(Input());

        Assert.That(result.Rows.Any(x => x.Name == "Вирізання шеврона"), Is.False);
    }

    [Test]
    public void HoopGlueRateWinsEvenWhenZero()
    {
        var settings = ZeroExtras() with
        {
            Hoops = [.. ZeroExtras().Hoops.Select(x => x.Id == "18" ? x with { GlueRate = 0m } : x)],
            Expenses = [.. ZeroExtras().Expenses.Select(x => x.Id == "glue" ? x with { Rate = 4m } : x)],
        };

        var glue = Price(Input(), settings).Rows.Single(x => x.Name == "Клей");

        Assert.That((glue.Rate, glue.Amount), Is.EqualTo(((decimal?)0m, 0m)));
    }

    [Test]
    public void EachBasisIsChargedOnce()
    {
        var settings = ZeroExtras() with
        {
            Expenses =
            [
                new("a", "За стежки", EmbroideryExpenseBasis.Stitches, 1m, true, EmbroideryExpenseMode.All),
                new("b", "За зап’ялювання", EmbroideryExpenseBasis.Hoop, 1m, true, EmbroideryExpenseMode.All),
                new("c", "За виріб", EmbroideryExpenseBasis.Unit, 1m, true, EmbroideryExpenseMode.All),
                new("d", "За годину машини", EmbroideryExpenseBasis.MachineHour, 60m, true, EmbroideryExpenseMode.All),
                new("e", "За замовлення", EmbroideryExpenseBasis.Order, 1m, true, EmbroideryExpenseMode.All),
            ],
        };

        var extras = Price(Input(machineMinutes: 90m), settings).Rows.Where(x => x.Kind == EmbroideryCostKind.Extra);

        Assert.That(extras.Select(x => x.Amount), Is.EqualTo(new[] { 500m, 50m, 50m, 90m, 1m }));
    }

    [Test]
    public void MissingMachineTimeIsReportedOnlyForAPositiveRate()
    {
        EmbroiderySettingsContent WithMachineRate(decimal rate) => ZeroExtras() with
        {
            Expenses = [new("m", "Машина", EmbroideryExpenseBasis.MachineHour, rate, true, EmbroideryExpenseMode.All)],
        };

        Assert.That(Price(Input(machineMinutes: null), WithMachineRate(60m)).Missing,
            Is.EqualTo(new[] { EmbroideryCalculator.MachineTimeMissing }));
        Assert.That(Price(Input(machineMinutes: null), WithMachineRate(0m)).Missing, Is.Empty);
    }

    [Test]
    public void BlankWiderThanTheRollIsAnError()
    {
        var settings = ZeroExtras() with { Stabilizer = new EmbroideryMaterial(20m, 20m) };

        Assert.That(EmbroideryCalculator.Calculate(Input(), settings, 1m).Error,
            Is.EqualTo("Заготовка ширша за рулон."));
    }

    [Test]
    public void ZeroQuantityOrUnknownHoopIsAnError()
    {
        Assert.That(EmbroideryCalculator.Calculate(Input(quantity: 0), ZeroExtras(), 1m).Error,
            Is.EqualTo(EmbroideryCalculator.QuantityRequired));
        Assert.That(EmbroideryCalculator.Calculate(Input(hoop: "99"), ZeroExtras(), 1m).Error,
            Is.EqualTo(EmbroideryCalculator.HoopNotFound));
    }

    [Test]
    public void DefaultsAreValidSettings() =>
        Assert.DoesNotThrow(() => EmbroiderySettingsRules.EnsureValid(EmbroiderySettingsDefaults.Content));
}
