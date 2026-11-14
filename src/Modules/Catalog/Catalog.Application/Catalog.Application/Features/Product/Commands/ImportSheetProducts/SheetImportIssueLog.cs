
namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>Collects blocking problems and non-blocking warnings in the order they are found.</summary>
public sealed class SheetImportIssueLog
{
    private readonly List<SheetImportIssue> _problems = [];
    private readonly List<SheetImportIssue> _warnings = [];

    public IReadOnlyList<SheetImportIssue> Problems => _problems;

    public IReadOnlyList<SheetImportIssue> Warnings => _warnings;

    public void Problem(string scope, int? rowNumber, string? code, string message)
        => _problems.Add(new SheetImportIssue(scope, rowNumber, code, message));

    public void Warning(string scope, int? rowNumber, string? code, string message)
        => _warnings.Add(new SheetImportIssue(scope, rowNumber, code, message));
}
