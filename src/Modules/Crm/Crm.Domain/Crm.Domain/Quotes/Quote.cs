namespace Crm.Domain.Quotes;

/// <summary>
/// A commercial proposal (КП). The author never changes; the responsible person sees and edits it (R-01, R-02).
/// <see cref="Version"/> grows with every change and guards against stale saves (R-12).
/// </summary>
public sealed class Quote
{
    private readonly List<QuoteItem> _items = [];

    private Quote() { }

    public Quote(QuoteNumber number, QuoteKind kind, QuotePerson author, QuoteHeader header,
        IEnumerable<QuoteItem> items, DateTimeOffset now)
    {
        Number = number.ToString();
        NumberYear = number.Year;
        NumberMonth = number.Month;
        NumberSequence = number.Sequence;
        Kind = kind;
        Status = QuoteStatus.Draft;
        AuthorUserId = author.UserId;
        AuthorName = author.Name;
        ResponsibleUserId = author.UserId;
        ResponsibleName = author.Name;
        CreatedAt = now;
        UpdatedAt = now;
        Version = 1;
        ApplyHeader(header);
        _items.AddRange(items);
    }

    public long Id { get; private set; }
    public string Number { get; private set; } = null!;
    public int NumberYear { get; private set; }
    public int NumberMonth { get; private set; }
    public int NumberSequence { get; private set; }
    public QuoteKind Kind { get; private set; }
    public QuoteStatus Status { get; private set; }

    /// <summary>«Внутрішня назва клієнта / запиту»; only staff see it.</summary>
    public string InternalName { get; private set; } = null!;

    /// <summary>A Crm customer (KeepinCRM client).</summary>
    public long? CustomerId { get; private set; }

    public string? RecipientName { get; private set; }
    public string? CompanyName { get; private set; }
    public string? ClientComment { get; private set; }
    public string? ManagerNote { get; private set; }
    public int AuthorUserId { get; private set; }
    public string AuthorName { get; private set; } = null!;
    public int ResponsibleUserId { get; private set; }
    public string ResponsibleName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>When the КП was last sent by Gmail (R-15); null for a draft.</summary>
    public DateTimeOffset? LastSentAt { get; private set; }

    public int Version { get; private set; }

    public IReadOnlyList<QuoteItem> Items => _items;

    /// <summary>Replaces the header and the whole item list in one change.</summary>
    public void Update(QuoteHeader header, IEnumerable<QuoteItem> items, DateTimeOffset now)
    {
        ApplyHeader(header);
        _items.Clear();
        _items.AddRange(items);
        Touch(now);
    }

    public void AssignResponsible(QuotePerson person, DateTimeOffset now)
    {
        ResponsibleUserId = person.UserId;
        ResponsibleName = person.Name;
        Touch(now);
    }

    /// <summary>Gmail accepted the letter: the КП is sent. It can still be edited and sent again.</summary>
    public void MarkSent(DateTimeOffset now)
    {
        Status = QuoteStatus.Sent;
        LastSentAt = now;
        Touch(now);
    }

    public void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version++;
    }

    private void ApplyHeader(QuoteHeader header)
    {
        if (string.IsNullOrWhiteSpace(header.InternalName))
            throw new ArgumentException("A КП needs an internal client or request name.", nameof(header));
        InternalName = header.InternalName.Trim();
        CustomerId = header.CustomerId;
        RecipientName = QuoteText.Normalize(header.RecipientName);
        CompanyName = QuoteText.Normalize(header.CompanyName);
        ClientComment = QuoteText.Normalize(header.ClientComment);
        ManagerNote = QuoteText.Normalize(header.ManagerNote);
    }
}

/// <summary>The fields above the items (R-04).</summary>
public sealed record QuoteHeader(
    string InternalName,
    long? CustomerId,
    string? RecipientName,
    string? CompanyName,
    string? ClientComment,
    string? ManagerNote);
