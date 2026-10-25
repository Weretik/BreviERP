namespace CatalogImport.Persistence;

public sealed class ReferenceImportResult
{
    public Dictionary<string, int> Suppliers { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> Fabrics { get; init; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> Accessories { get; init; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> Operations { get; init; } = new(StringComparer.Ordinal);
}

