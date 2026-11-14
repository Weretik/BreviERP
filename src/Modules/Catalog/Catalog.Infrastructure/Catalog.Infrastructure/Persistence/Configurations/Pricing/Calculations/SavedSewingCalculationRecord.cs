using Catalog.Domain.Products;
using Catalog.Domain.Products.Sewing.ValueObjects;
using Catalog.Domain.Products.ValueObjects;


namespace Catalog.Infrastructure.Persistence.Configurations.Pricing.Calculations;

internal sealed class SavedSewingCalculationRecord
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public Guid RequestId { get; set; }
    public ProductId ProductId { get; set; }
    public ArticleId ArticleId { get; set; }
    public string SnapshotJson { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
}
