namespace Crm.Domain.Quotes;

/// <summary>One line of «Історія змін» of a КП (R-22): what happened, who did it and when.</summary>
public sealed class QuoteEvent
{
    private QuoteEvent() { }

    internal QuoteEvent(QuoteEventKind kind, QuotePerson actor, string? details, DateTimeOffset at)
    {
        Kind = kind;
        ActorUserId = actor.UserId;
        ActorName = actor.Name;
        var text = QuoteText.Normalize(details);
        Details = text is { Length: > QuoteLimits.ReasonMaxLength } ? text[..QuoteLimits.ReasonMaxLength] : text;
        At = at;
    }

    public long Id { get; private set; }
    public long QuoteId { get; private set; }
    public QuoteEventKind Kind { get; private set; }
    public int ActorUserId { get; private set; }
    public string ActorName { get; private set; } = null!;

    /// <summary>A short note, for example the recipients of a letter or the new responsible person.</summary>
    public string? Details { get; private set; }

    public DateTimeOffset At { get; private set; }
}

/// <summary>What a history line of a КП records (R-22).</summary>
public enum QuoteEventKind
{
    Created = 1,
    Saved = 2,
    Sent = 3,
    Repriced = 4,
    ResponsibleChanged = 5,
    Archived = 6,
    Restored = 7,

    /// <summary>The КП was made as a copy of another one (R-24).</summary>
    Copied = 8
}
