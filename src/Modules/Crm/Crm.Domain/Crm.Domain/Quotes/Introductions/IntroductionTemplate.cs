using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;

namespace Crm.Domain.Quotes.Introductions;

public sealed class IntroductionTemplate : BaseEntity<IntroductionTemplateId>, IAggregateRoot
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public CrmUserId OwnerId { get; private set; }
    public long Revision { get; private set; } = 1;
    public int VersionNumber { get; private set; }
    public bool Archived { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private IntroductionTemplate() { }

    public static IntroductionTemplate Create(CrmUserId owner)
    {
        if (owner.Value <= 0) throw new IntroductionValidationException("invalid_template_owner");
        return new IntroductionTemplate { OwnerId = owner };
    }

    public void RecordVersion(long expectedRevision, IntroductionTemplateVersion version)
    {
        if (Revision != expectedRevision) throw new IntroductionValidationException("revision_conflict");
        if (Archived) throw new IntroductionValidationException("template_archived");
        if (Id.Value <= 0 || version.TemplateId != Id || version.Number != VersionNumber + 1)
            throw new IntroductionValidationException("invalid_template_version");
        VersionNumber = version.Number;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetArchived(long expectedRevision, bool archived)
    {
        if (Revision != expectedRevision) throw new IntroductionValidationException("revision_conflict");
        Archived = archived;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
