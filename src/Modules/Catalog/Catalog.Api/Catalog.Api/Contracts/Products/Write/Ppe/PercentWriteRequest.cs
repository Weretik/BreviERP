namespace Catalog.Api.Contracts.Products;

public sealed record PercentWriteRequest(string Source, int? AdditionalReferenceId, decimal? CustomPercent);
