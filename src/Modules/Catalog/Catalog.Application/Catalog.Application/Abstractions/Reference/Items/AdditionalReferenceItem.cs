namespace Catalog.Application.Abstractions.Reference.Items;

public sealed record AdditionalReferenceItem(int Id, string Name, string Key, decimal Value, string Unit);
