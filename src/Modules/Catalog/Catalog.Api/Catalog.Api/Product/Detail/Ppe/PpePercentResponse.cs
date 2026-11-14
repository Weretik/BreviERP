namespace Catalog.Api.Product.Detail.Ppe;

public sealed record PpePercentResponse(string Source, AdditionalReferenceResponse? Reference, decimal? CustomPercent);
