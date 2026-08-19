namespace Catalog.Api.Contracts.Products;

public sealed record PpePercentResponse(string Source, AdditionalReferenceResponse? Reference, decimal? CustomPercent);
