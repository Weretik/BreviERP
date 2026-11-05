namespace Production.Domain.Forecast;

/// <summary>«Прогноз пошиття» capacity: SEAMSTRESS_COUNT, MINUTES_SHIFT and MINUTE_COST_SEAMSTRESS.</summary>
public sealed record ForecastSettings(int SeamstressCount, decimal ShiftMinutes, decimal MinuteCost)
{
    public bool IsValid => SeamstressCount > 0 && ShiftMinutes > 0 && MinuteCost > 0;

    /// <summary>What the workshop sews in a working day, in UAH of sewing rate.</summary>
    public decimal DailyCapacity => SeamstressCount * ShiftMinutes * MinuteCost;
}

/// <summary>An item still to sew: pieces left and the rate per piece (null when the item has none).</summary>
public sealed record ForecastItemLoad(int Remaining, decimal? Rate);

public sealed record ForecastOrderLoad(int OrderId, IReadOnlyList<ForecastItemLoad> Items);

public enum ForecastStatus
{
    Estimated = 1,
    Complete = 2,
    Unknown = 3
}

public enum ForecastReason
{
    MissingRate = 1,
    UnknownPredecessor = 2,
    BeyondHorizon = 3
}

public sealed record ForecastLine(int OrderId, int Remaining, decimal? Minutes, DateOnly? FinishOn, ForecastStatus Status,
    ForecastReason? Reason);

/// <summary>
/// The approximate end of sewing of each order in the queue (PRD-D38): the work left in UAH of rate, added up along the
/// queue, divided by what the workshop sews a day, counted in working days Monday to Friday from the next working day.
/// An order with pieces but no rate has no date, and so has every order after it.
/// </summary>
public static class SewingForecast
{
    /// <summary>Ten years of working days at most.</summary>
    public const int MaxWorkingDays = 2610;

    public static DateOnly NextWorkingDay(DateOnly today)
    {
        var day = today.AddDays(1);
        while (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) day = day.AddDays(1);
        return day;
    }

    /// <summary>The day the n-th working day (1 = start) falls on.</summary>
    public static DateOnly AddWorkingDays(DateOnly start, int workingDays)
    {
        var day = start;
        for (var left = workingDays - 1; left > 0;)
        {
            day = day.AddDays(1);
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)) left--;
        }

        return day;
    }

    public static IReadOnlyList<ForecastLine> Calculate(IReadOnlyList<ForecastOrderLoad> queue, ForecastSettings settings,
        DateOnly start)
    {
        if (!settings.IsValid) throw new ArgumentException("Forecast settings must be positive.", nameof(settings));
        var lines = new List<ForecastLine>(queue.Count);
        var cumulative = 0m;
        var blocked = false;
        foreach (var order in queue)
        {
            var remaining = order.Items.Sum(x => Math.Max(0, x.Remaining));
            if (remaining == 0)
            {
                lines.Add(new ForecastLine(order.OrderId, 0, 0, null, ForecastStatus.Complete, null));
                continue;
            }

            if (order.Items.Any(x => x.Remaining > 0 && x.Rate is not > 0))
            {
                blocked = true;
                lines.Add(new ForecastLine(order.OrderId, remaining, null, null, ForecastStatus.Unknown, ForecastReason.MissingRate));
                continue;
            }

            var work = order.Items.Where(x => x.Remaining > 0).Sum(x => x.Remaining * x.Rate!.Value);
            var minutes = decimal.Round(work / settings.MinuteCost, 1);
            if (blocked)
            {
                lines.Add(new ForecastLine(order.OrderId, remaining, minutes, null, ForecastStatus.Unknown,
                    ForecastReason.UnknownPredecessor));
                continue;
            }

            cumulative += work;
            var days = decimal.Ceiling(cumulative / settings.DailyCapacity);
            if (days > MaxWorkingDays)
            {
                blocked = true;
                lines.Add(new ForecastLine(order.OrderId, remaining, minutes, null, ForecastStatus.Unknown, ForecastReason.BeyondHorizon));
                continue;
            }

            lines.Add(new ForecastLine(order.OrderId, remaining, minutes, AddWorkingDays(start, (int)Math.Max(1, days)),
                ForecastStatus.Estimated, null));
        }

        return lines;
    }
}
