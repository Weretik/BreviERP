namespace Catalog.Api.Contracts.Products;

public sealed record AdditionalReferenceResponse(int Id, string Name, string Key, decimal Value, string Unit);
