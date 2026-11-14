using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Payroll.Shared;

/// <summary>Months of «Зарплата» as «РРРР-ММ» and the lock of a closed month (PRD-D43).</summary>
public static class PayrollMonths
{
    public static bool TryParse(string? value, out DateOnly month)
    {
        month = default;
        return value is { Length: 7 } && DateOnly.TryParseExact($"{value}-01", "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out month);
    }

    public static DateOnly Of(DateOnly day) => new(day.Year, day.Month, 1);

    public static string Format(DateOnly month) => month.ToString("yyyy-MM", CultureInfo.InvariantCulture);

    public static DateOnly Current(TimeProvider clock) => Of(WorkshopDay.Today(clock));

    /// <summary>True when the month of <paramref name="day"/> is closed: nothing in it may change.</summary>
    public static Task<bool> IsClosedAsync(IProductionDbContext db, DateOnly day, CancellationToken cancellationToken)
    {
        var month = Of(day);
        return db.PayrollMonths.AnyAsync(x => x.Month == month && x.ClosedAt != null, cancellationToken);
    }

    /// <summary>The tracked month, created on its first change.</summary>
    public static async Task<PayrollMonth> TrackAsync(IProductionDbContext db, DateOnly month,
        CancellationToken cancellationToken)
    {
        var row = db.PayrollMonths.Local.FirstOrDefault(x => x.Month == month) ??
                  await db.PayrollMonths.SingleOrDefaultAsync(x => x.Month == month, cancellationToken);
        if (row is not null) return row;
        row = new PayrollMonth(month);
        db.PayrollMonths.Add(row);
        return row;
    }
}
