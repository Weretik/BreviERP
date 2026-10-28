namespace Catalog.Domain.Pricing.Sewing;

public sealed record SewingCustomCharges(
    decimal Delivery, decimal Packaging, decimal ApplicationCost, decimal ApplicationSale);
