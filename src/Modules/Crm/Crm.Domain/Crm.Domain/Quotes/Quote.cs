using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Domain.Quotes;

public sealed class Quote : BaseEntity<QuoteId>, IAggregateRoot
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public CustomerId? CustomerId { get; private set; }
    public Guid? ContactPublicId { get; private set; }
    public string RecipientName { get; private set; } = null!;
    public CrmUserId OwnerId { get; private set; }
    public CrmUserId CreatedBy { get; private set; }
    public long Revision { get; private set; } = 1;
    public int CurrentVersionNumber { get; private set; }
    public long? CurrentVersionId { get; private set; }
    public string State { get; private set; } = "draft";
    public bool Archived { get; private set; }
    public string? Number { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private Quote() { }

    public static Quote Create(CustomerId? customerId, Guid? contactPublicId, string recipientName, CrmUserId actor)
    {
        if (actor.Value <= 0 || customerId is { Value: <= 0 })
            throw new QuoteValidationException("invalid_owner_or_customer");
        var name = recipientName.Trim();
        if (name.Length is < 1 or > 300)
            throw new QuoteValidationException("recipient_name_invalid");
        return new Quote
        {
            CustomerId = customerId,
            ContactPublicId = contactPublicId,
            RecipientName = name,
            OwnerId = actor,
            CreatedBy = actor
        };
    }

    public void SetInitialVersion(long versionId)
    {
        if (CurrentVersionId is not null || versionId <= 0)
            throw new QuoteValidationException("invalid_initial_version");
        CurrentVersionId = versionId;
        CurrentVersionNumber = 1;
    }

    public void AppendVersion(long expectedRevision, long versionId)
    {
        if (expectedRevision != Revision || CurrentVersionId is null)
            throw new QuoteRevisionException();
        if (versionId <= 0) throw new QuoteValidationException("invalid_version");
        CurrentVersionId = versionId;
        CurrentVersionNumber++;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AssertEditable(long expectedRevision, int actorId, bool isAdmin)
    {
        if (State != "draft" || Archived || Revision != expectedRevision)
            throw new QuoteRevisionException();
        if (!isAdmin && OwnerId.Value != actorId)
            throw new QuoteValidationException("quote_inaccessible");
    }
}
