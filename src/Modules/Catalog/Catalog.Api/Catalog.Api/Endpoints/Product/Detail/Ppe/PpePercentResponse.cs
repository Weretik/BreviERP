namespace Catalog.Api.Endpoints.Product.Detail.Ppe;

public sealed record PpePercentResponse(string Source, AdditionalReferenceResponse? Reference, decimal? CustomPercent);
