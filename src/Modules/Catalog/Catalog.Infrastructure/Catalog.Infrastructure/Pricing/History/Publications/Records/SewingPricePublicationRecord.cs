namespace Catalog.Infrastructure.Pricing.History;

internal sealed class SewingPricePublicationRecord
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public long CalculationId { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public string PublishedBy { get; set; } = null!;
    public SavedSewingCalculationRecord Calculation { get; set; } = null!;
}
