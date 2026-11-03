namespace Catalog.Application.Contracts.Reference;

/// <param name="Code">The sheet code, such as FAB-023, FUR-005 or OP-018.</param>
public sealed record ReferenceCodeItem(int Id, string Code);
