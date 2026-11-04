using Crm.Domain.Embroidery.Settings;

namespace Crm.Domain.Embroidery.Calculation;

/// <summary>
/// The «Ціна вишивки» formula (EMB-001), the same as calculator.brevi.com.ua v217: a pure function of the form,
/// the settings and the embroiderer's rate from «Параметри ціноутворення».
/// </summary>
public static class EmbroideryCalculator
{
    public const string MachineTimeMissing = "Час роботи машини";
    public const string HoopNotFound = "Оберіть п’яльця.";
    public const string QuantityRequired = "Кількість: потрібне додатне ціле число.";
    public const string StitchesRequired = "Стежків на одиницю: потрібне додатне ціле число.";
    public const string LayersRequired = "Кількість шарів: потрібне додатне ціле число.";
    public const string PerHoopRequired = "Шевронів в одному зап’ялюванні: потрібне додатне ціле число.";

    private static readonly Rational Thousand = Rational.Of(1000);
    private static readonly Rational Hundred = Rational.Of(100);
    private static readonly Rational Sixty = Rational.Of(60);

    public static EmbroideryCalculation Calculate(EmbroideryInput input, EmbroiderySettingsContent settings,
        decimal stitchRate)
    {
        var chevron = input.Mode == EmbroideryMode.Chevron;
        if (input.Quantity <= 0) return EmbroideryCalculation.Failed(QuantityRequired);
        if (input.StitchesPerUnit <= 0) return EmbroideryCalculation.Failed(StitchesRequired);
        if (input.StabilizerLayers <= 0 || chevron && input.FabricLayers <= 0)
            return EmbroideryCalculation.Failed(LayersRequired);
        if (chevron && input.UnitsPerHoop <= 0) return EmbroideryCalculation.Failed(PerHoopRequired);
        var hoop = settings.Hoops.FirstOrDefault(x => x.Id == input.HoopId);
        if (hoop is null) return EmbroideryCalculation.Failed(HoopNotFound);

        long quantity = input.Quantity;
        long perHoop = chevron ? input.UnitsPerHoop : 1;
        var hoopings = (quantity + perHoop - 1) / perHoop;
        var lastHoopUnits = quantity % perHoop == 0 ? perHoop : quantity % perHoop;
        var thousands = Rational.Of(quantity * input.StitchesPerUnit) / Thousand;
        var units = Rational.Of(quantity);

        var programMinutes = input.Program == EmbroideryProgram.Ready ? 0 : Math.Max(0, input.ProgramMinutes);
        var programCost = Rational.Of(programMinutes) / Sixty * Rational.Of(settings.Rates.ProgramHourRate);

        var stabilizerWidth = Rational.Of(hoop.BlankWidthCm);
        var stabilizerLength = Rational.Of(hoop.BlankLengthCm);
        var stabilizer = EmbroideryCutLayout.Plan(settings.Stabilizer, stabilizerWidth, stabilizerLength,
            hoopings * input.StabilizerLayers, input.MaterialMode);
        if (stabilizer is null) return EmbroideryCalculation.Failed(EmbroideryCutLayout.BlankWiderThanRoll);

        (EmbroideryCut Cut, Rational Charged)? fabric = null;
        if (chevron)
        {
            var increase = Rational.One + Rational.Of(settings.FabricIncreasePercent) / Hundred;
            fabric = EmbroideryCutLayout.Plan(settings.Fabric, stabilizerWidth * increase, stabilizerLength * increase,
                hoopings * input.FabricLayers, input.MaterialMode);
            if (fabric is null) return EmbroideryCalculation.Failed(EmbroideryCutLayout.BlankWiderThanRoll);
        }

        var rows = new List<(EmbroideryCostKind Kind, string Name, EmbroideryExpenseBasis? Basis, Rational Quantity,
            decimal? Rate, Rational Amount)>
        {
            (EmbroideryCostKind.Stitches, "Оплата стежків", null, thousands, stitchRate, thousands * Rational.Of(stitchRate)),
            (EmbroideryCostKind.Threads, "Нитки", null, thousands, settings.Rates.ThreadRate,
                thousands * Rational.Of(settings.Rates.ThreadRate)),
            (EmbroideryCostKind.Hooping, "Зап’ялювання", null, Rational.Of(hoopings), settings.Rates.HoopingRate,
                Rational.Of(hoopings) * Rational.Of(settings.Rates.HoopingRate)),
            (EmbroideryCostKind.Stabilizer, "Флізелін", null, Rational.Of(stabilizer.Value.Cut.Blanks), null,
                stabilizer.Value.Charged),
        };
        if (fabric is { } chevronFabric)
            rows.Add((EmbroideryCostKind.Fabric, "Тканина", null, Rational.Of(chevronFabric.Cut.Blanks), null,
                chevronFabric.Charged));
        rows.Add((EmbroideryCostKind.Program, "Розробка програми", null, Rational.Of(programMinutes),
            settings.Rates.ProgramHourRate, programCost));

        var missing = new List<string>();
        foreach (var item in settings.Expenses)
        {
            var enabled = input.ExpenseOverrides.TryGetValue(item.Id, out var ticked) ? ticked : item.Enabled;
            if (!enabled || item.Mode == EmbroideryExpenseMode.ChevronOnly && !chevron) continue;

            var rate = item.Id == EmbroideryExpense.GlueId && hoop.GlueRate is not null ? hoop.GlueRate : item.Rate;
            if (rate is null)
            {
                missing.Add(item.Name);
                continue;
            }

            Rational baseQuantity;
            switch (item.Basis)
            {
                case EmbroideryExpenseBasis.Stitches: baseQuantity = thousands; break;
                case EmbroideryExpenseBasis.Hoop: baseQuantity = Rational.Of(hoopings); break;
                case EmbroideryExpenseBasis.Unit: baseQuantity = units; break;
                case EmbroideryExpenseBasis.Order: baseQuantity = Rational.One; break;
                default:
                    if (input.MachineMinutes is null && rate > 0)
                    {
                        missing.Add(MachineTimeMissing);
                        continue;
                    }

                    baseQuantity = Rational.Of(input.MachineMinutes ?? 0) / Sixty;
                    break;
            }

            rows.Add((EmbroideryCostKind.Extra, item.Name, item.Basis, baseQuantity, rate,
                baseQuantity * Rational.Of(rate.Value)));
        }

        var cost = rows.Aggregate(Rational.Zero, (sum, row) => sum + row.Amount);
        var tax = Rational.One + Rational.Of(settings.Rates.TaxPercent) / Hundred;
        var prices = settings.Markups.Select(markup =>
        {
            var factor = (Rational.One + Rational.Of(markup.Percent) / Hundred) * tax;
            var unit = (cost / units * factor).RoundHalfUp();
            var total = (Rational.Of(unit) * units).RoundHalfUp();
            var repeat = ((cost - programCost) / units * factor).RoundHalfUp();
            return new EmbroideryPrice(markup.Name, markup.Percent, unit, total, repeat);
        }).ToArray();

        return new EmbroideryCalculation(null, new EmbroideryResult(
            cost.RoundHalfUp(), (cost / units).RoundHalfUp(), checked((int)hoopings),
            checked((int)(quantity / perHoop)), checked((int)lastHoopUnits), checked((int)(perHoop - lastHoopUnits)),
            stabilizer.Value.Cut, fabric?.Cut,
            rows.Select(x => new EmbroideryCostRow(x.Kind, x.Name, x.Basis, x.Quantity.RoundHalfUp(), x.Rate,
                x.Amount.RoundHalfUp(), (x.Amount / units).RoundHalfUp())).ToArray(),
            prices, missing));
    }
}
