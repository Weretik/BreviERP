using Production.Domain.Orders;

namespace Production.Domain.Payroll;

/// <summary>
/// «Розрахунковий місяць» of «Зарплата» (PRD-D43): the embroidery of the month and whether the month is closed. A
/// closed month keeps the calculation as it was (<see cref="Snapshot"/>) and refuses any change of work, hours or pay
/// in it. <see cref="Month"/> is the first day of the month.
/// </summary>
public sealed class PayrollMonth
{
    public const int MaxStitches = 1_000_000_000;

    private PayrollMonth() { }

    public PayrollMonth(DateOnly month) => Month = new DateOnly(month.Year, month.Month, 1);

    public DateOnly Month { get; private set; }

    /// <summary>Stitches embroidered in the month; paid to <see cref="EmbroideryEmployeeId"/>.</summary>
    public int Stitches { get; private set; }

    public int? EmbroideryEmployeeId { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }
    public string? ClosedByName { get; private set; }

    /// <summary>The calculation of a closed month as JSON.</summary>
    public string? Snapshot { get; private set; }

    /// <summary>«Звіти цеху» of the month as it was closed, as JSON (PRD-D49).</summary>
    public string? ReportSnapshot { get; private set; }

    public int Version { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedByName { get; private set; }

    public bool IsClosed => ClosedAt is not null;

    public void SetEmbroidery(int stitches, int? employeeId, string actorName, DateTimeOffset now)
    {
        EnsureOpen();
        if (stitches is < 0 or > MaxStitches) throw new ArgumentOutOfRangeException(nameof(stitches));
        Stitches = stitches;
        EmbroideryEmployeeId = employeeId;
        Touch(actorName, now);
    }

    public void Close(string snapshot, string reportSnapshot, string actorName, DateTimeOffset now)
    {
        EnsureOpen();
        Snapshot = snapshot;
        ReportSnapshot = reportSnapshot;
        ClosedAt = now;
        ClosedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        Touch(actorName, now);
    }

    public void Reopen(string actorName, DateTimeOffset now)
    {
        if (!IsClosed) throw new InvalidOperationException("The month is open.");
        Snapshot = null;
        ReportSnapshot = null;
        ClosedAt = null;
        ClosedByName = null;
        Touch(actorName, now);
    }

    private void EnsureOpen()
    {
        if (IsClosed) throw new InvalidOperationException("The month is closed.");
    }

    private void Touch(string actorName, DateTimeOffset now)
    {
        UpdatedAt = now;
        UpdatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        Version++;
    }
}
