namespace Crm.Domain.Quotes;

/// <summary>
/// A price row of an item: a fabric variant of a sewing model, or the single row of a PPE item. Texts and source
/// prices are copied from the Catalog when the row is added and change only by an explicit recalculation (R-11).
/// </summary>
public sealed class QuoteItemOption
{
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

    /// <summary>Metres of <see cref="FabricId"/> in one «Виріб з нуля»; null for other items.</summary>
    public decimal? FabricMeters { get; private set; }

    public QuoteItemOption WithFabricMeters(decimal? meters)
    {
        FabricMeters = meters;
        return this;
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
