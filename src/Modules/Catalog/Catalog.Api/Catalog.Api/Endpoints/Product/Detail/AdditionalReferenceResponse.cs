namespace Catalog.Api.Endpoints.Product.Detail;

public sealed record AdditionalReferenceResponse(int Id, string Name, string Key, decimal Value, string Unit);
