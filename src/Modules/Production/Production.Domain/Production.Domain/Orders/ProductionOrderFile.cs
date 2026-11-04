namespace Production.Domain.Orders;

/// <summary>
/// A file of an order or of one of its items: sizes, application layout, an extra file or a photo. New files live on
/// the CDN in the order's folder; a file moved from the old «Цех» keeps only its Google Drive link.
/// </summary>
public sealed class ProductionOrderFile
{
    public const int NameMaxLength = 255;
    public const int ContentTypeMaxLength = 120;
    public const int UrlMaxLength = 1000;
    public const int StorageKeyMaxLength = 300;
    public const long MaxBytes = 20 * 1024 * 1024;

    private ProductionOrderFile() { }

    public ProductionOrderFile(int orderId, int? itemId, ProductionFileKind kind, string name, string contentType,
        long bytes, string url, string? storageKey, string actorName, DateTimeOffset createdAt)
    {
        OrderId = orderId;
        ItemId = itemId;
        Kind = kind;
        Name = OrderText.Required(name, NameMaxLength);
        ContentType = OrderText.Required(contentType, ContentTypeMaxLength);
        Bytes = bytes;
        Url = OrderText.Required(url, UrlMaxLength);
        StorageKey = OrderText.Optional(storageKey, StorageKeyMaxLength);
        CreatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }

    /// <summary>Null for a file of the whole order.</summary>
    public int? ItemId { get; private set; }

    public ProductionFileKind Kind { get; private set; }
    public string Name { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long Bytes { get; private set; }
    public string Url { get; private set; } = null!;

    /// <summary>The CDN key to delete the file by; null for an old Google Drive link.</summary>
    public string? StorageKey { get; private set; }

    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }

    public void Delete(DateTimeOffset now) => DeletedAt ??= now;
}
