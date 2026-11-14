
namespace Catalog.Application.Abstractions.Pricing.PpeSupplierPrices.Sources;

/// <summary>An uploaded supplier price file; the stream is read once.</summary>
public sealed record SupplierPriceFile(string Name, Stream Content);
