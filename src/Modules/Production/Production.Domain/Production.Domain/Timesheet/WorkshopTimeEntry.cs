using BuildingBlocks.Domain.Errors;
using BuildingBlocks.Domain.Results;
using Production.Domain.Orders;

namespace Production.Domain.Timesheet;

/// <summary>
/// «Робочий час»: the hours a worker actually worked on a day, in steps of 15 minutes (PRD-D41). 0 means the worker
/// was absent; no row means the day is not marked yet. One row per worker and day; every change is kept in
/// <see cref="WorkshopTimeChange"/>.
/// </summary>
public sealed class WorkshopTimeEntry
{
    public const int MaxMinutes = 1440;
    public const int Step = 15;

    private WorkshopTimeEntry() { }

    public static DomainResult<WorkshopTimeEntry> Create(int employeeId, DateOnly workDate, int minutes,
        string actorName, DateTimeOffset now)
    {
        if (!IsValid(minutes)) return MinutesInvalid();
        var entry = new WorkshopTimeEntry { EmployeeId = employeeId, WorkDate = workDate, Version = 1 };
        entry.Set(minutes, actorName, now);
        return entry;
    }

    public int Id { get; private set; }
    public int EmployeeId { get; private set; }
    public DateOnly WorkDate { get; private set; }
    public int Minutes { get; private set; }

    /// <summary>Grows on each change; a save with an older version is refused.</summary>
    public int Version { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }
    public string UpdatedByName { get; private set; } = null!;

    public static bool IsValid(int minutes) => minutes is >= 0 and <= MaxMinutes && minutes % Step == 0;

    public DomainResult Change(int minutes, string actorName, DateTimeOffset now)
    {
        if (!IsValid(minutes)) return MinutesInvalid();
        Set(minutes, actorName, now);
        Version++;
        return DomainResult.Success();
    }

    private static Error MinutesInvalid() =>
        Error.Validation("production.time_minutes", "Час — від 0 до 24 годин, кроком 15 хвилин.", "minutes");

    private void Set(int minutes, string actorName, DateTimeOffset now)
    {
        if (!IsValid(minutes)) throw new ArgumentOutOfRangeException(nameof(minutes));
        Minutes = minutes;
        UpdatedAt = now;
        UpdatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
    }
}

/// <summary>One change of «Робочий час»: who changed the hours of a worker on a day, from what to what.</summary>
public sealed class WorkshopTimeChange
{
    private WorkshopTimeChange() { }

    public WorkshopTimeChange(int employeeId, string employeeName, DateOnly workDate, int? beforeMinutes,
        int afterMinutes, string actorName, DateTimeOffset now)
    {
        EmployeeId = employeeId;
        EmployeeName = OrderText.Required(employeeName, ProductionOrder.PersonMaxLength);
        WorkDate = workDate;
        BeforeMinutes = beforeMinutes;
        AfterMinutes = afterMinutes;
        ChangedAt = now;
        ChangedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
    }

    public int Id { get; private set; }
    public int EmployeeId { get; private set; }
    public string EmployeeName { get; private set; } = null!;
    public DateOnly WorkDate { get; private set; }

    /// <summary>Null on the first mark of the day.</summary>
    public int? BeforeMinutes { get; private set; }

    public int AfterMinutes { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string ChangedByName { get; private set; } = null!;
}
