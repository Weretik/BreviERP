using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Identifiers;
using Crm.Domain.CustomSewing.Versions;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Domain.CustomSewing.Drafts;

public sealed class CustomSewingDraft : BaseEntity<CustomDraftId>, IAggregateRoot
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public CrmUserId OwnerId { get; private set; }
    public CustomerId? CustomerId { get; private set; }
    public Crm.Domain.Customers.Customer? Customer { get; private set; }
    public QuoteId? QuoteId { get; private set; }
    public Quote? Quote { get; private set; }
    public long Revision { get; private set; } = 1;
    public string Name { get; private set; } = null!;
    public string ProductType { get; private set; } = null!;
    public string Season { get; private set; } = null!;
    public CustomConfiguration Configuration { get; private set; } = null!;
    public CustomTemplateOrigin? TemplateOrigin { get; private set; }
    public CustomVersionId? CurrentVersionId { get; private set; }
    public CustomSewingVersion? CurrentVersion { get; private set; }
    public int VersionNumber { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;
    private CustomSewingDraft() { }

    public static CustomSewingDraft Create(CrmUserId owner, CustomerId? customerId, QuoteId? quoteId,
        CustomConfiguration configuration)
    {
        if (owner.Value <= 0 || customerId is { Value: <= 0 } || quoteId is { Value: <= 0 })
            throw new CustomSewingValidationException("invalid_custom_owner_or_link");
        var draft = new CustomSewingDraft { OwnerId = owner, CustomerId = customerId, QuoteId = quoteId };
        draft.SetConfiguration(configuration);
        return draft;
    }

    public void SetTemplateOrigin(CustomTemplateOrigin origin)
    {
        if (Id.Value != 0 || Revision != 1 || TemplateOrigin is not null || CurrentVersionId is not null)
            throw new CustomSewingValidationException("template_origin_already_fixed");
        TemplateOrigin = origin.Validate();
    }

    public void Revise(long expectedRevision, CustomConfiguration configuration)
    {
        if (Revision != expectedRevision) throw new CustomSewingRevisionException();
        SetConfiguration(configuration);
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetVersion(long expectedRevision, CustomSewingVersion version)
    {
        if (Revision != expectedRevision) throw new CustomSewingRevisionException();
        if (version.Id.Value <= 0 || version.DraftId != Id || version.Number != VersionNumber + 1
            || version.ParentVersionId != CurrentVersionId)
            throw new CustomSewingValidationException("invalid_custom_version_pointer");
        CurrentVersionId = version.Id;
        VersionNumber = version.Number;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void SetConfiguration(CustomConfiguration configuration)
    {
        var frozen = configuration.Freeze();
        Configuration = frozen;
        Name = frozen.Name;
        ProductType = frozen.ProductType;
        Season = frozen.Season;
    }
}
