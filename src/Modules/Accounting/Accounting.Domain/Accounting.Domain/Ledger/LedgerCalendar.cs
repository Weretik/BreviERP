namespace Accounting.Domain.Ledger;

/// <summary>Fixed boundary between the archive and the current books (01.10.2026, as in the old Calculator).</summary>
public static class LedgerCalendar
{
    public static readonly DateOnly CutoverDate = new(2026, 10, 1);

    public const string TimeZoneId = "Europe/Kyiv";

    /// <summary>The area a calendar day belongs to.</summary>
    public static LedgerScope ScopeOf(DateOnly date) => date < CutoverDate ? LedgerScope.LegacyArchive : LedgerScope.Current;
}
