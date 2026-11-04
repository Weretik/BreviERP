namespace Crm.Domain.Quotes;

/// <summary>
/// A price row of an item: a fabric variant of a sewing model, or the single row of a PPE item. Texts and source
/// prices are copied from the Catalog when the row is added and change only by an explicit recalculation (R-11).
/// </summary>
public sealed class QuoteItemOption
{
    private readonly List<QuoteItemOptionFabric> _fabrics = [];

    private QuoteItemOption() { }

    public QuoteItemOption(
        int position, long? articleId, int? fabricId, string title, string? details, bool isRecommended,
        QuotePrice retail, QuotePrice wholesale, QuotePrice large, DateTimeOffset? pricesTakenAt)
    {
        Position = position;
        ArticleId = articleId;
        FabricId = fabricId;
        Title = title.Trim();
        Details = QuoteText.Normalize(details);
        IsRecommended = isRecommended;
        Retail = retail;
        Wholesale = wholesale;
        Large = large;
        PricesTakenAt = pricesTakenAt;
    }

    public long Id { get; private set; }
    public long ItemId { get; private set; }
    public int Position { get; private set; }

    /// <summary>Catalog Article (fabric variant) of a sewing model; null for PPE.</summary>
    public long? ArticleId { get; private set; }

    /// <summary>Reference fabric of the variant.</summary>
    public int? FabricId { get; private set; }

    public string Title { get; private set; } = null!;

    /// <summary>Client text under the title: fabric composition, density and description.</summary>
    public string? Details { get; private set; }

    public bool IsRecommended { get; private set; }
    public QuotePrice Retail { get; private set; } = QuotePrice.Empty;
    public QuotePrice Wholesale { get; private set; } = QuotePrice.Empty;
    public QuotePrice Large { get; private set; } = QuotePrice.Empty;

    /// <summary>Why a manual PPE price was set (R-08), who and when.</summary>
    public string? ManualReason { get; private set; }
    public int? ManualByUserId { get; private set; }
    public string? ManualByName { get; private set; }
    public DateTimeOffset? ManualAt { get; private set; }

    /// <summary>When the source prices were copied from the Catalog.</summary>
    public DateTimeOffset? PricesTakenAt { get; private set; }

    /// <summary>Metres of <see cref="FabricId"/> in one product; null for a Catalog variant and a PPE.</summary>
    public decimal? FabricMeters { get; private set; }

    /// <summary>
    /// The fabrics a «Виріб з нуля» variant is sewn from, summed in its price (R-26), or the one fabric of a model row in
    /// another fabric (R-27); empty for a Catalog variant and a PPE. The first one is <see cref="FabricId"/>.
    /// </summary>
    public IReadOnlyList<QuoteItemOptionFabric> Fabrics => _fabrics;

    public QuoteItemOption WithFabrics(IEnumerable<QuoteItemOptionFabric> fabrics)
    {
        _fabrics.Clear();
        _fabrics.AddRange(fabrics);
        if (_fabrics.Count > 0) FabricId = _fabrics[0].FabricId;
        FabricMeters = _fabrics.Count > 0 ? _fabrics[0].Meters : null;
        return this;
    }

    /// <summary>True when the row is sewn from exactly these fabrics and metres, in this order.</summary>
    public bool HasFabrics(IReadOnlyList<(int FabricId, decimal Meters)> fabrics) =>
        _fabrics.Count == fabrics.Count &&
        _fabrics.OrderBy(x => x.Position).Select(x => (x.FabricId, x.Meters)).SequenceEqual(fabrics);

    /// <summary>The same row with its saved prices and manual price note, for a copy of the КП.</summary>
    public QuoteItemOption Copy()
    {
        var copy = new QuoteItemOption(Position, ArticleId, FabricId, Title, Details, IsRecommended, Retail, Wholesale,
            Large, PricesTakenAt) { FabricMeters = FabricMeters };
        copy._fabrics.AddRange(_fabrics.OrderBy(x => x.Position).Select(x => x.Copy()));
        copy.ManualReason = ManualReason;
        copy.ManualByUserId = ManualByUserId;
        copy.ManualByName = ManualByName;
        copy.ManualAt = ManualAt;
        return copy;
    }

    /// <summary>The same row with only the «показати клієнту» ticks changed (an embroidery row).</summary>
    public QuoteItemOption CopyShown(bool retail, bool wholesale, bool large)
    {
        var copy = Copy();
        copy.Retail = Retail with { Shown = retail };
        copy.Wholesale = Wholesale with { Shown = wholesale };
        copy.Large = Large with { Shown = large };
        return copy;
    }

    public IEnumerable<QuotePrice> Prices => [Retail, Wholesale, Large];

    public bool HasManualPrice => Prices.Any(x => x.IsManual);

    public bool ShowsPositivePrice => Prices.Any(x => x.IsVisiblePositive);

    public void SetManualAudit(string? reason, QuotePerson? person, DateTimeOffset? at)
    {
        ManualReason = QuoteText.Normalize(reason);
        ManualByUserId = person?.UserId;
        ManualByName = person?.Name;
        ManualAt = at;
    }

    /// <summary>New Catalog prices from «Перерахувати за актуальними цінами»; manual prices stay.</summary>
    public void ReplaceSourcePrices(decimal retail, decimal wholesale, decimal large, DateTimeOffset now)
    {
        Retail = Retail.WithSource(retail);
        Wholesale = Wholesale.WithSource(wholesale);
        Large = Large.WithSource(large);
        PricesTakenAt = now;
    }
}
