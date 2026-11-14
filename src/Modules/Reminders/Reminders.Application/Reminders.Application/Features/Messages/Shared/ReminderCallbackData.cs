using Reminders.Domain.Dispatching;

namespace Reminders.Application.Features.Messages.Shared;

/// <summary>Button payload "rm:{occurrenceId}:{action}", well under Telegram's 64-byte limit.</summary>
public readonly record struct ReminderCallbackData(long OccurrenceId, ReminderCallbackAction Action, SnoozeOption? Snooze = null)
{
    private const string Prefix = "rm";

    public override string ToString() => $"{Prefix}:{OccurrenceId}:{Action switch
    {
        ReminderCallbackAction.Done => "done",
        ReminderCallbackAction.Later => "later",
        ReminderCallbackAction.Back => "back",
        _ => Snooze switch { SnoozeOption.Tomorrow => "s1", SnoozeOption.TwoDays => "s2", _ => "s3" }
    }}";

    public static bool TryParse(string? value, out ReminderCallbackData data)
    {
        data = default;
        var parts = value?.Split(':');
        if (parts is not { Length: 3 } || parts[0] != Prefix || !long.TryParse(parts[1], out var id) || id <= 0) return false;
        ReminderCallbackData? parsed = parts[2] switch
        {
            "done" => new(id, ReminderCallbackAction.Done),
            "later" => new(id, ReminderCallbackAction.Later),
            "back" => new(id, ReminderCallbackAction.Back),
            "s1" => new(id, ReminderCallbackAction.Snooze, SnoozeOption.Tomorrow),
            "s2" => new(id, ReminderCallbackAction.Snooze, SnoozeOption.TwoDays),
            "s3" => new(id, ReminderCallbackAction.Snooze, SnoozeOption.Monday),
            _ => null
        };
        if (parsed is null) return false;
        data = parsed.Value;
        return true;
    }
}
