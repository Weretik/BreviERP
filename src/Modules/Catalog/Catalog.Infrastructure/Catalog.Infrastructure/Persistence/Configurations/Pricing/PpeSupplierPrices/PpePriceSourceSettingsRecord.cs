
namespace Catalog.Infrastructure.Persistence.Configurations.Pricing.PpeSupplierPrices;

/// <summary>Saved settings of one «Імпорт цін» source; the source list itself lives in code.</summary>
public sealed class PpePriceSourceSettingsRecord
{
    public string Key { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public decimal DiscountPercent { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
}
