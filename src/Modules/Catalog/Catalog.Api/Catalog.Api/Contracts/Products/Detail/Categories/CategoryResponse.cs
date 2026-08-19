namespace Catalog.Api.Contracts.Products;

public sealed record CategoryResponse(int Id, string Name, string RuName, string Slug);
