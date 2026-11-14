namespace Reference.Application.Features.SheetImport.Shared;

/// <summary>Collects the Ukrainian labels of fields whose sheet value differs from Brevi.</summary>
internal sealed class ReferenceImportDiff
{
    private readonly List<string> _changes = [];

    public IReadOnlyList<string> Changes => _changes;

    public ReferenceImportDiff Compare<T>(string label, T current, T incoming)
    {
        if (!EqualityComparer<T>.Default.Equals(current, incoming))
            _changes.Add(label);
        return this;
    }

    public ReferenceImportDiff Text(string label, string? current, string? incoming) =>
        Compare(label, Normalize(current), Normalize(incoming));

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
