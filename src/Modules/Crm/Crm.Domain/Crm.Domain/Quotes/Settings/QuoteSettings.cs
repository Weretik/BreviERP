namespace Crm.Domain.Quotes.Settings;

/// <summary>
/// «Налаштування КП» (R-13): one record for the company, edited only by Director and Admin.
/// <see cref="Version"/> guards against two people saving at once.
/// </summary>
public sealed class QuoteSettings
{
    public const int SingletonId = 1;

    private QuoteSettings() { }

    public QuoteSettings(QuoteSettingsContent content, string updatedByName, DateTimeOffset now)
    {
        Id = SingletonId;
        Version = 0;
        Apply(content, updatedByName, now);
    }

    public int Id { get; private set; }
    public QuoteCompanyDetails Company { get; private set; } = null!;
    public QuoteEmailTemplate Email { get; private set; } = null!;
    public QuotePdfTexts Pdf { get; private set; } = null!;
    public QuoteIntroductionTexts Introduction { get; private set; } = null!;
    public IReadOnlyList<QuoteLetterVariant> LetterVariants { get; private set; } = [];
    public IReadOnlyList<QuoteStaffContact> StaffContacts { get; private set; } = [];
    public string UpdatedByName { get; private set; } = null!;
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public QuoteSettingsContent Content =>
        new(Company, Email, Pdf, Introduction, LetterVariants, StaffContacts);

    public void Apply(QuoteSettingsContent content, string updatedByName, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(content.Company.Name))
            throw new ArgumentException("Company name is required.", nameof(content));
        if (content.Introduction.Advantages.Count != QuoteSettingsLimits.AdvantagesCount)
            throw new ArgumentException("The introduction catalogue has exactly five advantages.", nameof(content));
        if (content.StaffContacts.Select(x => x.UserId).Distinct().Count() != content.StaffContacts.Count)
            throw new ArgumentException("One contact per staff member.", nameof(content));

        Company = content.Company;
        Email = content.Email;
        Pdf = content.Pdf;
        Introduction = content.Introduction;
        LetterVariants = [.. content.LetterVariants];
        StaffContacts = [.. content.StaffContacts];
        UpdatedByName = updatedByName;
        UpdatedAt = now;
        Version++;
    }

    /// <summary>The saved contact of a staff member, or null to use their account name, phone and email.</summary>
    public QuoteStaffContact? ContactOf(int userId) => StaffContacts.FirstOrDefault(x => x.UserId == userId);
}

/// <summary>Everything the settings page edits.</summary>
public sealed record QuoteSettingsContent(
    QuoteCompanyDetails Company,
    QuoteEmailTemplate Email,
    QuotePdfTexts Pdf,
    QuoteIntroductionTexts Introduction,
    IReadOnlyList<QuoteLetterVariant> LetterVariants,
    IReadOnlyList<QuoteStaffContact> StaffContacts);
