using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;

namespace Crm.Domain.Documents;

public sealed class DocumentSettingsState : BaseEntity<DocumentSettingsStateId>, IAggregateRoot
{
    public long Revision { get; private set; } = 1;
    public IssuerSettingsVersionId? ActiveIssuerVersionId { get; private set; }
    public DocumentTemplateVersionId? ActiveTemplateVersionId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private DocumentSettingsState() { }

    public static DocumentSettingsState Create() => new() { Id = new DocumentSettingsStateId(1) };

    public void Activate(long expectedRevision, IssuerSettingsVersionId issuerId, DocumentTemplateVersionId templateId)
    {
        if (Revision != expectedRevision) throw new DocumentSettingsValidationException("revision_conflict");
        if (issuerId.Value <= 0 || templateId.Value <= 0)
            throw new DocumentSettingsValidationException("invalid_active_settings");
        ActiveIssuerVersionId = issuerId;
        ActiveTemplateVersionId = templateId;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
