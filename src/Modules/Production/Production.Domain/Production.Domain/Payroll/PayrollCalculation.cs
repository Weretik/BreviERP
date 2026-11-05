using Production.Domain.Work;

namespace Production.Domain.Payroll;

/// <summary>
/// Shares from «Параметри ціноутворення» as fractions (0.08 = 8 %); null when the value is not filled in.
/// </summary>
public sealed record PayrollRates(decimal? Bonus, decimal? BonusThreshold, decimal? Cutting, decimal? Master,
    decimal? Chief, decimal? Embroidery);

/// <summary>Not cancelled work of the month with the rate it is paid by.</summary>
public sealed record PayrollWorkLine(int EmployeeId, ProductionWorkKind Kind, int Quantity, decimal Rate);

public sealed record PayrollPeople(int? MasterId, int? ChiefId, int? EmbroideryId);

/// <summary>Everything the month is paid from.</summary>
public sealed record PayrollInputs(
    IReadOnlyCollection<int> EmployeeIds,
    IReadOnlyList<PayrollWorkLine> Work,
    IReadOnlyDictionary<int, decimal> Adjustments,
    IReadOnlyDictionary<int, decimal> Payments,
    PayrollPeople People,
    int Stitches,
    PayrollRates Rates);

public sealed record PayrollAmounts(int EmployeeId, int Sewn, decimal Sewing, int Cut, decimal Cutting, decimal Bonus,
    decimal Master, decimal Chief, decimal Embroidery, decimal Other, decimal Paid)
{
    public decimal Accrued => Sewing + Cutting + Bonus + Master + Chief + Embroidery + Other;

    public decimal Due => Accrued - Paid;
}

public enum PayrollWarning
{
    NoMaster = 1,
    NoChief = 2,
    NoEmbroideryWorker = 3,
    NoBonusRate = 4,
    NoBonusThreshold = 5,
    NoCuttingRate = 6,
    NoMasterRate = 7,
    NoChiefRate = 8,
    NoEmbroideryRate = 9
}

public sealed record PayrollResult(IReadOnlyList<PayrollAmounts> Lines, decimal TotalSewing,
    IReadOnlyList<PayrollWarning> Warnings);

/// <summary>
/// The pay of a month (PRD-D42), as the old «Цех» counted it: sewing = pieces × rate; cutting = pieces × rate × the
/// cutting share; the seamstress bonus is a share of her sewing once it reaches the threshold; the master and the chief
/// get a share of the sewing of all workers; embroidery = stitches / 1000 × the rate; adjustments add or deduct.
/// Each part is rounded to kopecks. A warning blocks closing the month.
/// </summary>
public static class PayrollCalculation
{
    public static PayrollResult Calculate(PayrollInputs inputs)
    {
        var rates = inputs.Rates;
        var people = inputs.People;
        var warnings = Warnings(inputs).ToList();
        var work = inputs.Work.ToLookup(x => x.EmployeeId);
        var totalSewing = inputs.Work.Where(x => x.Kind == ProductionWorkKind.Sewing).Sum(x => Money(x.Quantity * x.Rate));
        var ids = inputs.EmployeeIds
            .Concat(inputs.Work.Select(x => x.EmployeeId))
            .Concat(inputs.Adjustments.Keys)
            .Concat(inputs.Payments.Keys)
            .Concat(new[] { people.MasterId, people.ChiefId, inputs.Stitches > 0 ? people.EmbroideryId : null }
                .OfType<int>())
            .Distinct();

        var lines = ids.Select(id =>
        {
            var own = work[id].ToList();
            var sewingLines = own.Where(x => x.Kind == ProductionWorkKind.Sewing).ToList();
            var cuttingLines = own.Where(x => x.Kind == ProductionWorkKind.Cutting).ToList();
            var sewing = sewingLines.Sum(x => Money(x.Quantity * x.Rate));
            var cutting = cuttingLines.Sum(x => Money(x.Quantity * x.Rate * (rates.Cutting ?? 0)));
            var bonus = rates is { Bonus: { } share, BonusThreshold: { } threshold } && sewing > 0 && sewing >= threshold
                ? Money(sewing * share)
                : 0;
            return new PayrollAmounts(id,
                sewingLines.Sum(x => x.Quantity), sewing,
                cuttingLines.Sum(x => x.Quantity), cutting,
                bonus,
                id == people.MasterId ? Money(totalSewing * (rates.Master ?? 0)) : 0,
                id == people.ChiefId ? Money(totalSewing * (rates.Chief ?? 0)) : 0,
                id == people.EmbroideryId ? Money(inputs.Stitches * (rates.Embroidery ?? 0) / 1000m) : 0,
                inputs.Adjustments.GetValueOrDefault(id),
                inputs.Payments.GetValueOrDefault(id));
        }).ToList();
        return new PayrollResult(lines, totalSewing, warnings);
    }

    private static IEnumerable<PayrollWarning> Warnings(PayrollInputs inputs)
    {
        var rates = inputs.Rates;
        var hasSewing = inputs.Work.Any(x => x.Kind == ProductionWorkKind.Sewing);
        var hasCutting = inputs.Work.Any(x => x.Kind == ProductionWorkKind.Cutting);
        if (hasSewing && rates.Bonus is null) yield return PayrollWarning.NoBonusRate;
        if (hasSewing && rates.BonusThreshold is null) yield return PayrollWarning.NoBonusThreshold;
        if (hasCutting && rates.Cutting is null) yield return PayrollWarning.NoCuttingRate;
        if (hasSewing && rates.Master is null) yield return PayrollWarning.NoMasterRate;
        if (hasSewing && rates.Chief is null) yield return PayrollWarning.NoChiefRate;
        if (inputs.Stitches > 0 && rates.Embroidery is null) yield return PayrollWarning.NoEmbroideryRate;
        if (hasSewing && rates.Master is > 0 && inputs.People.MasterId is null) yield return PayrollWarning.NoMaster;
        if (hasSewing && rates.Chief is > 0 && inputs.People.ChiefId is null) yield return PayrollWarning.NoChief;
        if (inputs.Stitches > 0 && inputs.People.EmbroideryId is null) yield return PayrollWarning.NoEmbroideryWorker;
    }

    private static decimal Money(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
