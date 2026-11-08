namespace Accounting.Domain.Movements;

/// <summary>
/// What a movement of the old Calculator «Фінанси» keeps when it is moved over (stage 5): where it came from, its state
/// and version, and who made it when. <see cref="ExternalId"/> and <see cref="Fingerprint"/> are the identity of a
/// bank or «Домашня бухгалтерія» line, kept so a later statement import does not add it twice.
/// </summary>
public sealed record LegacyMovement(
    MovementSource Source,
    MovementStatus Status,
    int Version,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? ExternalId,
    string? Fingerprint)
{
    public const int ExternalIdMaxLength = 200;
    public const int FingerprintMaxLength = 128;

    /// <summary>Checks the parts every moved record shares.</summary>
    public void Check()
    {
        if (!Enum.IsDefined(Source) || !Enum.IsDefined(Status))
            throw new ArgumentOutOfRangeException(nameof(Source), "Unknown source or status of a moved record.");
        if (Version < 1) throw new ArgumentOutOfRangeException(nameof(Version), "Version starts at 1.");
        if (string.IsNullOrWhiteSpace(CreatedBy) || CreatedBy.Length > MovementRules.ByMaxLength)
            throw new ArgumentException("Author is required.", nameof(CreatedBy));
        if (ExternalId?.Length > ExternalIdMaxLength || Fingerprint?.Length > FingerprintMaxLength)
            throw new ArgumentException("External id or fingerprint is too long.", nameof(ExternalId));
    }
}
