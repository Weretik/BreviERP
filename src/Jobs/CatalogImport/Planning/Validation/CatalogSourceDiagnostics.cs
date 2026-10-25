using CatalogImport.Source;

namespace CatalogImport.Planning;

public sealed class CatalogSourceDiagnostics
{
    private readonly List<string> _errors = [];
    public List<string> Warnings { get; } = [];

    public void Require(bool condition, WorkbookRow row, string message)
    {
        if (!condition)
            _errors.Add($"{row.Key}: {message}.");
    }

    public void Error(string message) => _errors.Add(message);

    public void ThrowIfInvalid()
    {
        if (_errors.Count != 0)
            throw new InvalidDataException(string.Join(Environment.NewLine, _errors));
    }
}

