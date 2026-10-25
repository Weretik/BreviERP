namespace Catalog.Domain.Pricing.Sewing;

public sealed record SewingPriceTiers(decimal Retail, decimal Wholesale, decimal LargeWholesale)
{
    public decimal ForQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        return quantity <= 9 ? Retail : quantity <= 40 ? Wholesale : LargeWholesale;
    }
}

