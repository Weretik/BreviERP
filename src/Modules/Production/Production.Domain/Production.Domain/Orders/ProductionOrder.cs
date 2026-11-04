using System.Globalization;

namespace Production.Domain.Orders;

/// <summary>
/// «Виробниче завдання»: what the workshop sews for one КП. The manager creates it from their sent КП («Передати в
/// цех»); the client, manager and every item are copied then. <see cref="CreateRequestId"/> makes a repeated press of
/// the button return the same order instead of a duplicate.
/// </summary>
public sealed class ProductionOrder
{
    public const int ClientMaxLength = 300;
    public const int PersonMaxLength = 200;
    public const int QuoteNumberMaxLength = 40;
    public const int CommentMaxLength = 4000;
    public const int LegacyIdMaxLength = 64;
    public const int MaxItems = 100;

    private readonly List<ProductionItem> _items = [];
    private readonly List<ProductionOrderChange> _changes = [];

    private ProductionOrder() { }

    public ProductionOrder(int number, OrderOrigin origin, OrderHeader header, ReportPriceTier? priceTier,
        IEnumerable<ProductionItem> items, string actorName, DateTimeOffset now)
    {
        Number = number;
        QuoteId = origin.QuoteId;
        QuoteNumber = OrderText.Optional(origin.QuoteNumber, QuoteNumberMaxLength);
        CustomerId = origin.CustomerId;
        ManagerUserId = origin.ManagerUserId;
        ManagerName = OrderText.Required(origin.ManagerName, PersonMaxLength);
        CreateRequestId = origin.RequestId;
        LegacyId = OrderText.Optional(origin.LegacyId, LegacyIdMaxLength);
        CopiedFromOrderId = origin.CopiedFromOrderId;
        PriceTier = priceTier;
        Status = ProductionOrderStatus.New;
        ApplyHeader(header);
        _items.AddRange(items);
        if (_items.Count is 0 or > MaxItems) throw new ArgumentException("An order needs from 1 to 100 items.", nameof(items));
        CreatedAt = now;
        CreatedByName = OrderText.Required(actorName, PersonMaxLength);
        UpdatedAt = now;
        Version = 1;
    }

    public int Id { get; private set; }

    /// <summary>Order number shown to people; the old app's numbers are kept on transfer.</summary>
    public int Number { get; private set; }

    /// <summary>The Crm КП; null for an order moved from the old app, which keeps only <see cref="QuoteNumber"/>.</summary>
    public long? QuoteId { get; private set; }

    public string? QuoteNumber { get; private set; }
    public long? CustomerId { get; private set; }

    /// <summary>The client as the workshop sees it; may differ from the КП (another company name for this order).</summary>
    public string ClientName { get; private set; } = null!;

    public int? ManagerUserId { get; private set; }
    public string ManagerName { get; private set; } = null!;
    public ProductionOrderStatus Status { get; private set; }

    /// <summary>«Дата створення» the manager gives the order; it may be earlier than the record.</summary>
    public DateOnly OrderDate { get; private set; }

    public string? Comment { get; private set; }
    public ReportPriceTier? PriceTier { get; private set; }
    public Guid? CreateRequestId { get; private set; }

    /// <summary>Id of the order in the old «Цех»; null for orders made in Brevi.</summary>
    public string? LegacyId { get; private set; }

    public int? CopiedFromOrderId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public int Version { get; private set; }

    public IReadOnlyList<ProductionItem> Items => _items;

    /// <summary>History lines added in this change; saved ones are read by a separate query.</summary>
    public IReadOnlyList<ProductionOrderChange> Changes => _changes;

    public bool IsDeleted => DeletedAt is not null;

    /// <summary>Sets the status of an order moved from the old app.</summary>
    public void SetTransferredStatus(ProductionOrderStatus status) => Status = status;

    /// <summary>
    /// Replaces the header and the details of the given items; every changed field becomes a history line. Items
    /// keep their source from the КП.
    /// </summary>
    /// <returns>False when nothing changed.</returns>
    public bool Update(OrderHeader header, IReadOnlyDictionary<int, ItemUpdate> items, string actorName,
        DateTimeOffset now)
    {
        var before = _changes.Count;
        Track(null, null, "Клієнт", ClientName, OrderText.Required(header.ClientName, ClientMaxLength), actorName, now);
        Track(null, null, "Дата створення", Format(OrderDate), Format(header.OrderDate), actorName, now);
        Track(null, null, "Коментар", Comment, OrderText.Optional(header.Comment, CommentMaxLength), actorName, now);
        ApplyHeader(header);

        foreach (var item in _items)
        {
            if (!items.TryGetValue(item.Id, out var update)) continue;
            var old = item.Details;
            var next = update.Details;
            var name = item.ModelName;
            Track(item.Id, name, "Кількість", old.Quantity.ToString(CultureInfo.InvariantCulture),
                next.Quantity.ToString(CultureInfo.InvariantCulture), actorName, now);
            Track(item.Id, name, "Готово до", Format(old.DueDate), Format(next.DueDate), actorName, now);
            Track(item.Id, name, "Основний колір", old.MainColor, next.MainColor.Trim(), actorName, now);
            Track(item.Id, name, "Додаткові кольори", Join(old.AdditionalColors), Join(Clean(next.AdditionalColors)),
                actorName, now);
            Track(item.Id, name, "Розміри", old.SizesText, OrderText.Optional(next.SizesText, ProductionItem.LongTextMaxLength),
                actorName, now);
            Track(item.Id, name, "Технічні зміни", old.TechnicalChanges,
                OrderText.Optional(next.TechnicalChanges, ProductionItem.LongTextMaxLength), actorName, now);
            Track(item.Id, name, "Коментар", old.Comment, OrderText.Optional(next.Comment, ProductionItem.LongTextMaxLength),
                actorName, now);
            Track(item.Id, name, "Нанесення", Describe(old.Applications), Describe(next.Applications), actorName, now);
            item.Update(next, update.PlannedFabricMeters);
            if (update.ChangesSewingRate)
            {
                Track(item.Id, name, "Розцінка пошиття", Money(item.SewingRate), Money(update.SewingRate), actorName, now);
                item.SetSewingRate(update.SewingRate);
            }
        }

        if (_changes.Count == before) return false;
        Touch(now);
        return true;
    }

    /// <summary>«Видалити»: the order leaves every list; its history stays.</summary>
    public void Delete(string actorName, DateTimeOffset now)
    {
        if (IsDeleted) return;
        DeletedAt = now;
        _changes.Add(new ProductionOrderChange(null, null, "Видалено", null, "Так", actorName, now));
        Touch(now);
    }

    public void Record(string field, string? after, string actorName, DateTimeOffset now) =>
        _changes.Add(new ProductionOrderChange(null, null, field, null, after, actorName, now));

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version++;
    }

    private void ApplyHeader(OrderHeader header)
    {
        ClientName = OrderText.Required(header.ClientName, ClientMaxLength);
        OrderDate = header.OrderDate;
        Comment = OrderText.Optional(header.Comment, CommentMaxLength);
    }

    private void Track(int? itemId, string? itemName, string field, string? before, string? after, string actorName,
        DateTimeOffset now)
    {
        if (string.Equals(before ?? string.Empty, after ?? string.Empty, StringComparison.Ordinal)) return;
        _changes.Add(new ProductionOrderChange(itemId, itemName, field, before, after, actorName, now));
    }

    private static IEnumerable<string> Clean(IEnumerable<string> colors) =>
        colors.Select(x => x.Trim()).Where(x => x.Length > 0);

    private static string Join(IEnumerable<string> values) => string.Join(", ", values);

    private static string Format(DateOnly date) => date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

    private static string? Money(decimal? value) => value?.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Describe(IEnumerable<ItemApplication> applications) => string.Join("; ", applications.Select(x =>
        string.Join(", ", new[]
        {
            x.Kind == ItemApplicationKind.Embroidery ? "Вишивка" : "Друк",
            x.Place?.Trim(), x.Description?.Trim(), x.Color?.Trim()
        }.Where(part => !string.IsNullOrEmpty(part)))));
}

/// <summary>Where the order came from: the КП, a copy, or the old app; set once.</summary>
public sealed record OrderOrigin(
    long? QuoteId,
    string? QuoteNumber,
    long? CustomerId,
    int? ManagerUserId,
    string ManagerName,
    Guid? RequestId,
    string? LegacyId = null,
    int? CopiedFromOrderId = null);

/// <summary>The fields above the items.</summary>
public sealed record OrderHeader(string ClientName, DateOnly OrderDate, string? Comment);

/// <param name="ChangesSewingRate">Only Admin and the head of the workshop may change the rate.</param>
public sealed record ItemUpdate(ItemDetails Details, decimal? PlannedFabricMeters, bool ChangesSewingRate,
    decimal? SewingRate);
