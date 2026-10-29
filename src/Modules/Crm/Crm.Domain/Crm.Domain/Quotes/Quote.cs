using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Entity;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Events;

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
    public long? IssuedVersionId { get; private set; }
    public string State { get; private set; } = "draft";
    public bool Archived { get; private set; }
    public string? Number { get; private set; }
    public Guid? SourceQuotePublicId { get; private set; }
    public Guid? SourceVersionPublicId { get; private set; }
    private readonly List<QuoteLifecycleEvent> _history = [];
    public IReadOnlyCollection<QuoteLifecycleEvent> History => _history;
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

    public void AppendVersion(long expectedRevision, long versionId, int actorId, string action = "edited")
    {
        if (actorId <= 0 || action is not ("edited" or "recalculated"))
            throw new QuoteValidationException("invalid_version_audit");
        if (Archived || expectedRevision != Revision || CurrentVersionId is null)
            throw new QuoteRevisionException();
        if (versionId <= 0) throw new QuoteValidationException("invalid_version");
        if (State is "ready" or "sent") IssuedVersionId ??= CurrentVersionId;
        CurrentVersionId = versionId;
        CurrentVersionNumber++;
        State = "draft";
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
        Record(action, actorId);
    }

    public void PrepareReady(long expectedRevision, string number, int actorId, bool isAdmin)
    {
        AssertEditable(expectedRevision, actorId, isAdmin);
        if (CurrentVersionId is null || State != "draft")
            throw new QuoteRevisionException();
        if (string.IsNullOrWhiteSpace(number) || number.Length > 80 ||
            (Number is not null && Number != number))
            throw new QuoteValidationException("invalid_quote_number");
        Number ??= number;
        IssuedVersionId = CurrentVersionId;
        State = "ready";
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
        Record("readied", actorId);
    }

    public void AssertEditable(long expectedRevision, int actorId, bool isAdmin)
    {
        if (actorId <= 0) throw new QuoteValidationException("invalid_actor");
        if (Archived || Revision != expectedRevision)
            throw new QuoteRevisionException();
        if (!isAdmin && OwnerId.Value != actorId)
            throw new QuoteValidationException("quote_inaccessible");
    }

    public void Record(string action, int actorId, string? reason = null, CrmUserId? previousOwner = null)
        => _history.Add(QuoteLifecycleEvent.Create(Revision, CurrentVersionNumber, action,
            new CrmUserId(actorId), OwnerId, previousOwner, reason));

    public void SetCopySource(Guid sourceQuoteId, Guid sourceVersionId)
    {
        if (CurrentVersionId is not null || sourceQuoteId == Guid.Empty || sourceVersionId == Guid.Empty)
            throw new QuoteValidationException("invalid_copy_source");
        SourceQuotePublicId = sourceQuoteId;
        SourceVersionPublicId = sourceVersionId;
    }

    public void Transfer(long expectedRevision, CrmUserId target, int actorId, bool isAdmin, string reason)
    {
        AssertEditable(expectedRevision, actorId, isAdmin);
        if (target.Value <= 0 || target == OwnerId || string.IsNullOrWhiteSpace(reason) || reason.Length > 1000)
            throw new QuoteValidationException("invalid_transfer");
        var previous = OwnerId;
        OwnerId = target;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
        Record("transferred", actorId, reason.Trim(), previous);
    }

    public void SetArchived(long expectedRevision, bool archived, int actorId, bool isAdmin, string reason)
    {
        if (actorId <= 0) throw new QuoteValidationException("invalid_actor");
        if (Revision != expectedRevision) throw new QuoteRevisionException();
        if ((!isAdmin && OwnerId.Value != actorId) || archived == Archived ||
            string.IsNullOrWhiteSpace(reason) || reason.Length > 1000)
            throw new QuoteValidationException("invalid_archive");
        Archived = archived;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
        Record(archived ? "archived" : "restored", actorId, reason.Trim());
    }
}
