using Reminders.Domain.Chats;
using Reminders.Domain.Occurrences;
using Reminders.Domain.Schedules;
using Reminders.Domain.Tasks;

namespace Reminders.Application.Contracts;

/// <summary>Stable string codes used in the HTTP contract instead of numeric enums.</summary>
public static class ReminderCodes
{
    public static readonly IReadOnlyDictionary<string, ReminderOrganization> Organizations = new Dictionary<string, ReminderOrganization>
    {
        ["none"] = ReminderOrganization.None,
        ["ppBrevi"] = ReminderOrganization.PpBrevi,
        ["fopSohokon"] = ReminderOrganization.FopSohokon
    };

    public static readonly IReadOnlyDictionary<string, ReminderPeriodicity> Periodicities = new Dictionary<string, ReminderPeriodicity>
    {
        ["monthly"] = ReminderPeriodicity.Monthly,
        ["quarterly"] = ReminderPeriodicity.Quarterly,
        ["yearly"] = ReminderPeriodicity.Yearly
    };

    public static readonly IReadOnlyDictionary<string, DueRuleKind> DueRules = new Dictionary<string, DueRuleKind>
    {
        ["dayOfMonth"] = DueRuleKind.DayOfMonth,
        ["lastWorkingDayOfMonth"] = DueRuleKind.LastWorkingDayOfMonth,
        ["daysAfterPeriodEnd"] = DueRuleKind.DaysAfterPeriodEnd
    };

    public static string Code<T>(IReadOnlyDictionary<string, T> map, T value) where T : struct, Enum =>
        map.First(x => EqualityComparer<T>.Default.Equals(x.Value, value)).Key;

    public static string Status(ReminderOccurrenceStatus status) => status == ReminderOccurrenceStatus.Done ? "done" : "open";

    public static string ChatKind(ReminderChatKind kind) => kind == ReminderChatKind.Group ? "group" : "private";

    public static string Action(ReminderActionKind kind) => kind switch
    {
        ReminderActionKind.Done => "done",
        ReminderActionKind.Postponed => "postponed",
        _ => "reopened"
    };
}
