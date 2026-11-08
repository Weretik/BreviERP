namespace Crm.Domain.Leads;

/// <summary>Length limits of a lead (LEAD-001). Sheet texts longer than a limit are cut, never refused.</summary>
public static class LeadLimits
{
    public const int ShortText = 200;
    public const int LongText = 2000;
    public const int CategoryMaxLength = 10;
    public const int NameMaxLength = 200;
    public const int CommentMaxLength = 2000;
    public const int ResultNameMaxLength = 60;
    public const int CategoryDescriptionMaxLength = 200;
    public const int MaxRetryDays = 60;
    public const int MaxResults = 30;

    public static string? Cut(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var text = value.Trim();
        return text.Length <= max ? text : text[..max];
    }
}
