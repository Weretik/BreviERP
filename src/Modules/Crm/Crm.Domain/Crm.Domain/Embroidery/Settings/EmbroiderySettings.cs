using BuildingBlocks.Domain.Results;

namespace Crm.Domain.Embroidery.Settings;

/// <summary>
/// «Ціна вишивки › Налаштування» (EMB-001): one record for the company, edited only by Director and Admin.
/// <see cref="Version"/> guards against two people saving at once.
/// </summary>
public sealed class EmbroiderySettings
{
    public const int SingletonId = 1;

    private EmbroiderySettings() { }

    public static DomainResult<EmbroiderySettings> Create(EmbroiderySettingsContent content, string updatedByName, DateTimeOffset now)
    {
        var settings = new EmbroiderySettings { Id = SingletonId, Version = 0 };
        var applied = settings.Apply(content, updatedByName, now);
        return applied.IsFailure ? applied.Error! : settings;
    }

    public int Id { get; private set; }
    public EmbroiderySettingsContent Content { get; private set; } = null!;
    public string UpdatedByName { get; private set; } = null!;
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public DomainResult Apply(EmbroiderySettingsContent content, string updatedByName, DateTimeOffset now)
    {
        var valid = EmbroiderySettingsRules.Check(content);
        if (valid.IsFailure) return valid;
        Content = content;
        UpdatedByName = updatedByName;
        UpdatedAt = now;
        Version++;
        return DomainResult.Success();
    }
}
