using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Identifiers;

namespace Crm.Domain.CustomSewing.Templates;

public sealed class CustomTemplate : BaseEntity<CustomTemplateId>, IAggregateRoot
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public CrmUserId OwnerId { get; private set; }
    public long Revision { get; private set; } = 1;
    public int VersionNumber { get; private set; }
    public bool Archived { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    private CustomTemplate() { }

    public static CustomTemplate Create(CrmUserId owner)
    {
        if (owner.Value <= 0) throw new CustomSewingValidationException("invalid_template_owner");
        return new CustomTemplate { OwnerId = owner };
    }

    public void RecordVersion(long expectedRevision, CustomTemplateVersion version)
    {
        if (Revision != expectedRevision) throw new CustomSewingRevisionException();
        if (Archived) throw new CustomSewingValidationException("template_archived");
        if (Id.Value <= 0 || version.TemplateId != Id || version.Number != VersionNumber + 1)
            throw new CustomSewingValidationException("invalid_template_version");
        VersionNumber = version.Number;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetArchived(long expectedRevision, bool archived)
    {
        if (Revision != expectedRevision) throw new CustomSewingRevisionException();
        Archived = archived;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
