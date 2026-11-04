namespace Production.Domain.Orders;

/// <summary>
/// A position of a production order: one model in one fabric. <see cref="Source"/> is copied from the КП and Catalog when
/// the order is created, so a later change of the КП or the model does not change the order.
/// </summary>
public sealed class ProductionItem
{
    public const int NameMaxLength = 300;
    public const int CodeMaxLength = 32;
    public const int ColorMaxLength = 80;
    public const int MaxAdditionalColors = 2;
    public const int LongTextMaxLength = 4000;
    public const int MaxApplications = 10;
    public const int MaxQuantity = 100_000;

    private List<string> _additionalColors = [];
    private List<ItemApplication> _applications = [];
    private List<ProductionStage> _stages = [ProductionStage.New];

    private ProductionItem() { }

    public ProductionItem(int position, ItemSource source, ItemDetails details, decimal? plannedFabricMeters)
    {
        Position = position;
        QuoteItemId = source.QuoteItemId;
        QuoteOptionId = source.QuoteOptionId;
        ProductId = source.ProductId;
        ArticleId = source.ArticleId;
        ModelCode = OrderText.Optional(source.ModelCode, CodeMaxLength)?.ToUpperInvariant();
        ModelName = OrderText.Required(source.ModelName, NameMaxLength);
        FabricId = source.FabricId;
        FabricCode = OrderText.Optional(source.FabricCode, CodeMaxLength)?.ToUpperInvariant();
        FabricName = OrderText.Optional(source.FabricName, NameMaxLength);
        SewingRate = source.SewingRate;
        ReportUnitPrice = source.ReportUnitPrice;
        Apply(details);
        PlannedFabricMeters = plannedFabricMeters;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int Position { get; private set; }
    public long? QuoteItemId { get; private set; }
    public long? QuoteOptionId { get; private set; }
    public int? ProductId { get; private set; }
    public long? ArticleId { get; private set; }

    /// <summary>Catalog model code (MOD-…); null for a «Виріб з нуля» of the КП.</summary>
    public string? ModelCode { get; private set; }

    public string ModelName { get; private set; } = null!;
    public int? FabricId { get; private set; }

    /// <summary>Reference fabric code (FAB-…).</summary>
    public string? FabricCode { get; private set; }

    public string? FabricName { get; private set; }

    /// <summary>«Розцінка пошиття», UAH per item: the seamstress labour of the model variant. Not for managers.</summary>
    public decimal? SewingRate { get; private set; }

    /// <summary>Price per item from the КП for the owner's reports. Only Admin and Director see it.</summary>
    public decimal? ReportUnitPrice { get; private set; }

    public int Quantity { get; private set; }

    /// <summary>«Готово до».</summary>
    public DateOnly DueDate { get; private set; }

    public string MainColor { get; private set; } = null!;
    public IReadOnlyList<string> AdditionalColors => _additionalColors;
    public string? SizesText { get; private set; }
    public string? TechnicalChanges { get; private set; }
    public string? Comment { get; private set; }
    public IReadOnlyList<ItemApplication> Applications => _applications;

    /// <summary>Metres the norm plans for the whole quantity; null when the model has no norm.</summary>
    public decimal? PlannedFabricMeters { get; private set; }

    /// <summary>The ticked етапи in workshop order; never empty.</summary>
    public IReadOnlyList<ProductionStage> Stages => _stages;

    /// <summary>The furthest ticked stage; colours the row and sorts the list.</summary>
    public ProductionStage CurrentStage { get; private set; } = ProductionStage.New;

    /// <summary>Colours the norm is chosen by: the main one plus the additional ones.</summary>
    public int ColorCount => 1 + _additionalColors.Count;

    public ItemDetails Details => new(Quantity, DueDate, MainColor, [.. _additionalColors], SizesText, TechnicalChanges,
        Comment, [.. _applications]);

    public ItemSource Source => new(QuoteItemId, QuoteOptionId, ProductId, ArticleId, ModelCode, ModelName, FabricId,
        FabricCode, FabricName, SewingRate, ReportUnitPrice);

    public void Update(ItemDetails details, decimal? plannedFabricMeters)
    {
        Apply(details);
        PlannedFabricMeters = plannedFabricMeters;
    }

    public void SetSewingRate(decimal? rate) => SewingRate = rate;

    /// <returns>False when the stages did not change.</returns>
    public bool SetStages(IEnumerable<ProductionStage> stages)
    {
        var next = ProductionStages.Normalize(stages);
        if (next.SequenceEqual(_stages)) return false;
        _stages = next;
        CurrentStage = next[^1];
        return true;
    }

    private void Apply(ItemDetails details)
    {
        if (details.Quantity is <= 0 or > MaxQuantity)
            throw new ArgumentException("Quantity must be positive.", nameof(details));
        var additional = details.AdditionalColors
            .Select(x => OrderText.Optional(x, ColorMaxLength)).OfType<string>().ToList();
        if (additional.Count > MaxAdditionalColors)
            throw new ArgumentException("At most two additional colours.", nameof(details));
        if (details.Applications.Count > MaxApplications)
            throw new ArgumentException("Too many applications.", nameof(details));

        Quantity = details.Quantity;
        DueDate = details.DueDate;
        MainColor = OrderText.Required(details.MainColor, ColorMaxLength);
        _additionalColors = additional;
        SizesText = OrderText.Optional(details.SizesText, LongTextMaxLength);
        TechnicalChanges = OrderText.Optional(details.TechnicalChanges, LongTextMaxLength);
        Comment = OrderText.Optional(details.Comment, LongTextMaxLength);
        _applications = details.Applications
            .Select(x => ItemApplication.Create(x.Kind, x.Place, x.Description, x.Color)).ToList();
    }
}

/// <summary>What the КП and Catalog gave the item when the order was created.</summary>
public sealed record ItemSource(
    long? QuoteItemId,
    long? QuoteOptionId,
    int? ProductId,
    long? ArticleId,
    string? ModelCode,
    string ModelName,
    int? FabricId,
    string? FabricCode,
    string? FabricName,
    decimal? SewingRate,
    decimal? ReportUnitPrice);

/// <summary>What the manager or the head of the workshop fill in for an item.</summary>
public sealed record ItemDetails(
    int Quantity,
    DateOnly DueDate,
    string MainColor,
    IReadOnlyList<string> AdditionalColors,
    string? SizesText,
    string? TechnicalChanges,
    string? Comment,
    IReadOnlyList<ItemApplication> Applications);
