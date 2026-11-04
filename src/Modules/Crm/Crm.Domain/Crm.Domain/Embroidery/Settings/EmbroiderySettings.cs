namespace Crm.Domain.Embroidery.Settings;

/// <summary>
/// «Ціна вишивки › Налаштування» (EMB-001): one record for the company, edited only by Director and Admin.
/// <see cref="Version"/> guards against two people saving at once.
/// </summary>
public sealed class EmbroiderySettings
{
    public const int SingletonId = 1;

    private EmbroiderySettings() { }

    public EmbroiderySettings(EmbroiderySettingsContent content, string updatedByName, DateTimeOffset now)
    {
        Id = SingletonId;
        Version = 0;
        Apply(content, updatedByName, now);
    }

    public int Id { get; private set; }
    public EmbroiderySettingsContent Content { get; private set; } = null!;
    public string UpdatedByName { get; private set; } = null!;
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public void Apply(EmbroiderySettingsContent content, string updatedByName, DateTimeOffset now)
    {
        EmbroiderySettingsRules.EnsureValid(content);
        Content = content;
        UpdatedByName = updatedByName;
        UpdatedAt = now;
        Version++;
    }
}
