namespace Crm.Domain.Tasks;

/// <summary>One line of the task checklist («Кроки»).</summary>
public sealed class ManagerTaskStep
{
    private ManagerTaskStep() { }

    internal ManagerTaskStep(int position, string text, bool isDone)
    {
        Position = position;
        Text = text;
        IsDone = isDone;
    }

    public long Id { get; private set; }
    public long TaskId { get; private set; }
    public int Position { get; private set; }
    public string Text { get; private set; } = null!;
    public bool IsDone { get; private set; }

    internal void Mark(bool isDone) => IsDone = isDone;
}
