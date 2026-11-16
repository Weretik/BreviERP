namespace ActivityLog.Domain.Entries;

/// <summary>One changed field of an entry: label and display values, cut to their limits.</summary>
public sealed record ActivityChange(string Field, string? From, string? To)
{
    public const int FieldMaxLength = 100;
    public const int ValueMaxLength = 200;

    public static ActivityChange Of(string field, string? from, string? to) =>
        new(ActivityText.Cut(field, FieldMaxLength), ActivityText.CutOrNull(from, ValueMaxLength),
            ActivityText.CutOrNull(to, ValueMaxLength));
}
