namespace Catalog.Api.Endpoints.Product.Detail;

public sealed record PpePercentResponse(string Source, AdditionalReferenceResponse? Reference, decimal? CustomPercent);
