namespace Crm.Domain.Quotes;

/// <summary>A product line of a КП with its price rows (fabric variants or the PPE row).</summary>
public sealed class QuoteItem
{
    private readonly List<QuoteItemOption> _options = [];
    private readonly List<QuoteItemLine> _lines = [];

    private QuoteItem() { }

    public QuoteItem(
        int position, QuoteItemKind kind, int? productId, QuoteItemTexts texts, IEnumerable<QuoteItemOption> options,
        IEnumerable<QuoteItemLine>? lines = null)
    {
        Position = position;
        Kind = kind;
        ProductId = productId;
        Code = QuoteText.Normalize(texts.Code);
        Name = texts.Name.Trim();
        Subtitle = QuoteText.Normalize(texts.Subtitle);
        Description = QuoteText.Normalize(texts.Description);
        SiteUrl = QuoteText.Normalize(texts.SiteUrl);
        PhotoUrl = QuoteText.Normalize(texts.PhotoUrl);
        Season = QuoteText.Normalize(texts.Season);
        SupplierUrl = QuoteText.Normalize(texts.SupplierUrl);
        _options.AddRange(options);
        if (lines is not null) _lines.AddRange(lines);
    }

    public long Id { get; private set; }
    public long QuoteId { get; private set; }
    public int Position { get; private set; }
    public QuoteItemKind Kind { get; private set; }

    /// <summary>Catalog product; null for a one-off PPE.</summary>
    public int? ProductId { get; private set; }

    public string? Code { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Subtitle { get; private set; }

    /// <summary>«Опис позиції у КП»: copied from the Catalog quote description, editable in this КП only (R-06).</summary>
    public string? Description { get; private set; }

    public string? SiteUrl { get; private set; }
    public string? PhotoUrl { get; private set; }
    public string? Season { get; private set; }
    public string? SupplierUrl { get; private set; }

    public IReadOnlyList<QuoteItemOption> Options => _options;

    /// <summary>Accessories and operations of a «Виріб з нуля», shared by all its fabrics; empty otherwise.</summary>
    public IReadOnlyList<QuoteItemLine> Lines => _lines;

    /// <summary>R-10: the client must see at least one positive price of the item.</summary>
    public bool ShowsPositivePrice => _options.Any(x => x.ShowsPositivePrice);
}

/// <summary>Display texts of an item.</summary>
public sealed record QuoteItemTexts(
    string? Code,
    string Name,
    string? Subtitle,
    string? Description,
    string? SiteUrl,
    string? PhotoUrl,
    string? Season,
    string? SupplierUrl);
