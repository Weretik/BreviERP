namespace Crm.Domain.Quotes;

/// <summary>
/// A commercial proposal (КП). The author never changes; the responsible person sees and edits it (R-01, R-02).
/// <see cref="Version"/> grows with every change and guards against stale saves (R-12).
/// </summary>
public sealed class Quote
{
    private readonly List<QuoteItem> _items = [];
    private readonly List<QuoteEvent> _events = [];

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
        Record(QuoteEventKind.Created, author, null, now);
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

    /// <summary>Pages of a «КП для знайомства»; null for a КП «під запит» (R-16).</summary>
    public QuoteIntroLayout? IntroLayout { get; private set; }

    /// <summary>Where the introduction is going to be sent (R-17).</summary>
    public string? RecipientEmail { get; private set; }

    /// <summary>The letter variant chosen for the introduction (R-17, R-18).</summary>
    public string? LetterName { get; private set; }

    /// <summary>The introduction fields together; null for a КП «під запит».</summary>
    public QuoteIntroduction? Introduction =>
        IntroLayout is { } layout ? new QuoteIntroduction(layout, RecipientEmail, LetterName) : null;

    /// <summary>When the КП was last sent by Gmail (R-15); null for a draft.</summary>
    public DateTimeOffset? LastSentAt { get; private set; }

    /// <summary>When the КП was moved to «Архів» (R-23); null while it is in work.</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    public string? ArchivedByName { get; private set; }

    public bool IsArchived => ArchivedAt is not null;

    public int Version { get; private set; }

    public IReadOnlyList<QuoteItem> Items => _items;

    /// <summary>History lines added in this change; saved ones are read by a separate query.</summary>
    public IReadOnlyList<QuoteEvent> Events => _events;

    /// <summary>
    /// «Копіювати КП» (R-24): a new draft with the same header, introduction and items with their saved prices. The
    /// person who copies becomes its author and responsible.
    /// </summary>
    public Quote CopyAs(QuoteNumber number, QuotePerson author, DateTimeOffset now)
    {
        var copy = new Quote(number, Kind, author,
            new QuoteHeader(InternalName, CustomerId, RecipientName, CompanyName, ClientComment, ManagerNote),
            _items.OrderBy(x => x.Position).Select(x => x.Copy()), now);
        if (Introduction is { } introduction) copy.SetIntroduction(introduction);
        copy._events.Clear(); // the copy starts its history with «Копія КП N» instead of «Створено»
        copy.Record(QuoteEventKind.Copied, author, $"Копія КП {Number}", now);
        return copy;
    }

    /// <summary>
    /// A КП transferred from the old calculator (R-25): its number, people, dates, status and items as they were there,
    /// with its history: created, the given lines (letters, transfers) and the import line with <paramref name="note"/>.
    /// </summary>
    public static Quote Import(QuoteImportedState state, QuoteHeader header, IEnumerable<QuoteItem> items,
        QuoteIntroduction? introduction, IEnumerable<QuoteImportedEvent> events, QuotePerson importedBy, string note,
        DateTimeOffset now)
    {
        var quote = new Quote(state.Number, state.Kind, state.Author, header, items, state.CreatedAt)
        {
            ResponsibleUserId = state.Responsible.UserId,
            ResponsibleName = state.Responsible.Name,
            UpdatedAt = state.UpdatedAt,
        };
        if (introduction is not null) quote.SetIntroduction(introduction);
        if (state.Sent)
        {
            quote.Status = QuoteStatus.Sent;
            quote.LastSentAt = state.LastSentAt ?? state.UpdatedAt;
        }

        foreach (var line in events) quote.Record(line.Kind, line.Actor, line.Details, line.At);
        quote.Record(QuoteEventKind.Imported, importedBy, note, now);
        return quote;
    }

    /// <summary>Adds a history line; the caller records what the change was.</summary>
    public void Record(QuoteEventKind kind, QuotePerson actor, string? details, DateTimeOffset at) =>
        _events.Add(new QuoteEvent(kind, actor, details, at));

    /// <summary>«В архів» (R-23): the КП leaves the working list and cannot change until it is restored.</summary>
    public void Archive(QuotePerson person, DateTimeOffset now)
    {
        if (IsArchived) return;
        ArchivedAt = now;
        ArchivedByName = person.Name;
        Touch(now);
        Record(QuoteEventKind.Archived, person, null, now);
    }

    public void Restore(QuotePerson person, DateTimeOffset now)
    {
        if (!IsArchived) return;
        ArchivedAt = null;
        ArchivedByName = null;
        Touch(now);
        Record(QuoteEventKind.Restored, person, null, now);
    }

    /// <summary>Replaces the header and the whole item list in one change.</summary>
    public void Update(QuoteHeader header, IEnumerable<QuoteItem> items, DateTimeOffset now)
    {
        ApplyHeader(header);
        _items.Clear();
        _items.AddRange(items);
        Touch(now);
    }

    /// <summary>The fields only an introduction has; the caller replaces the header and items in the same change.</summary>
    public void SetIntroduction(QuoteIntroduction introduction)
    {
        if (Kind != QuoteKind.Introduction)
            throw new InvalidOperationException("Only a «КП для знайомства» has a layout.");
        IntroLayout = introduction.Layout;
        RecipientEmail = QuoteText.Normalize(introduction.RecipientEmail)?.ToLowerInvariant();
        LetterName = QuoteText.Normalize(introduction.LetterName);
    }

    public void AssignResponsible(QuotePerson person, QuotePerson by, DateTimeOffset now)
    {
        var before = ResponsibleName;
        ResponsibleUserId = person.UserId;
        ResponsibleName = person.Name;
        Touch(now);
        Record(QuoteEventKind.ResponsibleChanged, by, $"{before} → {person.Name}", now);
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

/// <summary>The layout, recipient address and letter of a «КП для знайомства» (R-16, R-17).</summary>
public sealed record QuoteIntroduction(QuoteIntroLayout Layout, string? RecipientEmail, string? LetterName)
{
    public IReadOnlyList<QuoteIntroSeason> Pages => Layout switch
    {
        QuoteIntroLayout.Summer => [QuoteIntroSeason.Summer],
        QuoteIntroLayout.Winter => [QuoteIntroSeason.Winter],
        _ => [QuoteIntroSeason.Summer, QuoteIntroSeason.Winter],
    };
}

/// <summary>Who and when of a КП from the old calculator (R-25).</summary>
public sealed record QuoteImportedState(
    QuoteNumber Number,
    QuoteKind Kind,
    QuotePerson Author,
    QuotePerson Responsible,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool Sent,
    DateTimeOffset? LastSentAt);

/// <summary>A history line of a КП from the old calculator: a letter or a transfer.</summary>
public sealed record QuoteImportedEvent(QuoteEventKind Kind, QuotePerson Actor, string? Details, DateTimeOffset At);

/// <summary>The fields above the items (R-04).</summary>
public sealed record QuoteHeader(
    string InternalName,
    long? CustomerId,
    string? RecipientName,
    string? CompanyName,
    string? ClientComment,
    string? ManagerNote);
