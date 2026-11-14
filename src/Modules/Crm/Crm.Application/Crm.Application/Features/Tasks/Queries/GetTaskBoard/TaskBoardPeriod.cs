namespace Crm.Application.Features.Tasks.Queries.GetTaskBoard;

/// <summary>
/// The «Період» of «Мої завдання» (client corrections round 2, item 10): which done tasks the board shows, by the time
/// they were done. Open tasks are always shown.
/// </summary>
public enum TaskBoardPeriod
{
    /// <summary>«Цей тиждень»: since Monday 00:00 Kyiv time.</summary>
    Week = 0,

    /// <summary>«Минулий тиждень»: the Monday-to-Sunday week before.</summary>
    LastWeek = 1,

    /// <summary>«Останній місяць»: the last 30 days.</summary>
    Month = 2,

    /// <summary>«Квартал»: the last 90 days.</summary>
    Quarter = 3,

    /// <summary>«Рік»: the last 365 days.</summary>
    Year = 4,

    /// <summary>«Усі»: every done task, up to the board cap.</summary>
    All = 5,
}

/// <summary>The time range of a <see cref="TaskBoardPeriod"/>; <c>To</c> is exclusive, null means now.</summary>
public static class TaskBoardPeriods
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static (DateTimeOffset? From, DateTimeOffset? To) Range(TaskBoardPeriod period, DateTimeOffset now)
    {
        var monday = KyivMonday(now);
        return period switch
        {
            TaskBoardPeriod.Week => (monday, null),
            TaskBoardPeriod.LastWeek => (KyivMonday(monday.AddDays(-1)), monday),
            TaskBoardPeriod.Month => (now.AddDays(-30), null),
            TaskBoardPeriod.Quarter => (now.AddDays(-90), null),
            TaskBoardPeriod.Year => (now.AddDays(-365), null),
            _ => (null, null),
        };
    }

    /// <summary>Monday 00:00 in Kyiv of the week that holds <paramref name="at"/>, as a UTC instant.</summary>
    private static DateTimeOffset KyivMonday(DateTimeOffset at)
    {
        var local = TimeZoneInfo.ConvertTime(at, Kyiv);
        var days = ((int)local.DayOfWeek + 6) % 7;
        var start = local.Date.AddDays(-days);
        return new DateTimeOffset(start, Kyiv.GetUtcOffset(start)).ToUniversalTime();
    }
}
