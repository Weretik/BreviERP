namespace Reminders.Domain.Occurrences;

/// <summary>Who pressed a button or acted in Admin. The name is kept as it was at that moment.</summary>
public sealed record ReminderActor(string Name, long? TelegramUserId, int? AdminUserId)
{
    public const int NameMaxLength = 200;

    private ReminderActor() : this(string.Empty, null, null) { }

    public static ReminderActor Telegram(long userId, string? firstName, string? lastName, string? username)
    {
        var name = string.Join(' ', new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        if (name.Length == 0) name = string.IsNullOrWhiteSpace(username) ? $"id {userId}" : "@" + username;
        return new ReminderActor(Truncate(name), userId, null);
    }

    public static ReminderActor Admin(int? userId, string? name) =>
        new(Truncate(string.IsNullOrWhiteSpace(name) ? "Адмінка" : name.Trim()), null, userId);

    private static string Truncate(string value) => value.Length <= NameMaxLength ? value : value[..NameMaxLength];
}
