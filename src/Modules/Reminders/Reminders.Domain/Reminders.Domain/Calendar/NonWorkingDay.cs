namespace Reminders.Domain.Calendar;

/// <summary>An extra day off besides Saturday and Sunday, for example a restored public holiday.</summary>
public sealed class NonWorkingDay
{
    private NonWorkingDay() { }

    public NonWorkingDay(DateOnly date, string? note)
    {
        Date = date;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public DateOnly Date { get; private set; }
    public string? Note { get; private set; }
}
