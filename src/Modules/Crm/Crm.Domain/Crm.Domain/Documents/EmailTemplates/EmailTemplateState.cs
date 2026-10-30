using BuildingBlocks.Domain.Entity;

namespace Crm.Domain.Documents.EmailTemplates;

public sealed class EmailTemplateState : BaseEntity<EmailTemplateStateId>
{
    public long Revision { get; private set; } = 1;
    public EmailTemplateVersionId? ActiveVersionId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    private EmailTemplateState() { }
    public static EmailTemplateState Create() => new() { Id = new EmailTemplateStateId(1) };
    public void Activate(long expectedRevision, EmailTemplateVersionId versionId)
    {
        if (Revision != expectedRevision) throw new EmailTemplateValidationException("revision_conflict");
        if (versionId.Value <= 0) throw new EmailTemplateValidationException("email_template_version_invalid");
        ActiveVersionId = versionId;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
