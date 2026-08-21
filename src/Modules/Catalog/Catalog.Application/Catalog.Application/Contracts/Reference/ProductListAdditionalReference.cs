namespace Catalog.Application.Contracts.Reference;

public sealed record ProductListAdditionalReference(
    int Id,
    string Key,
    decimal Value,
    string Unit);
