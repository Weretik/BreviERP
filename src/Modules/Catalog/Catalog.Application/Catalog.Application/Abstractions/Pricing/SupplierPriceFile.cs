
namespace Catalog.Application.Abstractions.Pricing;

/// <summary>An uploaded supplier price file; the stream is read once.</summary>
public sealed record SupplierPriceFile(string Name, Stream Content);
