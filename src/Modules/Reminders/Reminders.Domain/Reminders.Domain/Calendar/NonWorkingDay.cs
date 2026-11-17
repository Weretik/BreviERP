using BuildingBlocks.Domain.Entity;
namespace Reminders.Domain.Calendar;

/// <summary>An extra day off besides Saturday and Sunday, for example a restored public holiday.</summary>
public sealed class NonWorkingDay : IAggregateRoot
{
    private NonWorkingDay() { }

    private NonWorkingDay(DateOnly date, string? note)
    {
        Date = date;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public static NonWorkingDay Create(DateOnly date, string? note) =>
        new(date, note);

    public DateOnly Date { get; private set; }
    public string? Note { get; private set; }
}
