using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Domain.Quotes.Events;

public sealed class QuoteLifecycleEvent
{
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public QuoteId QuoteId { get; private set; }
    public long Revision { get; private set; }
    public int VersionNumber { get; private set; }
    public string Action { get; private set; } = null!;
    public CrmUserId ActorId { get; private set; }
    public CrmUserId OwnerId { get; private set; }
    public CrmUserId? PreviousOwnerId { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private QuoteLifecycleEvent() { }

    internal static QuoteLifecycleEvent Create(long revision, int version, string action,
        CrmUserId actor, CrmUserId owner, CrmUserId? previousOwner, string? reason)
    {
        if (actor.Value <= 0 || revision <= 0 || version <= 0 ||
            action is not ("created" or "edited" or "recalculated" or "copied" or "transferred" or "archived" or "restored"))
            throw new QuoteValidationException("invalid_history_event");
        return new QuoteLifecycleEvent
        {
            Revision = revision,
            VersionNumber = version,
            Action = action,
            ActorId = actor,
            OwnerId = owner,
            PreviousOwnerId = previousOwner,
            Reason = reason
        };
    }
}
