namespace Host.Seed.Jobs.CrmHistory;

internal static class CrmHistoryOutputPaths
{
    public static (string ReportPath, string? LedgerPath) Validate(string freeze, string sourceRoot, string report, string? ledger)
    {
        var source = Path.GetFullPath(sourceRoot);
        var input = Path.GetFullPath(freeze);
        var reportPath = ValidateOne(report, input, source);
        var ledgerPath = string.IsNullOrWhiteSpace(ledger) ? null : ValidateOne(ledger, input, source);
        if (ledgerPath is not null && string.Equals(reportPath, ledgerPath, Comparison))
            throw new InvalidOperationException("Report and staging ledger must use distinct output paths.");
        return (reportPath, ledgerPath);
    }

    private static string ValidateOne(string value, string freeze, string sourceRoot)
    {
        var output = Path.GetFullPath(value);
        var prefix = Path.TrimEndingDirectorySeparator(sourceRoot) + Path.DirectorySeparatorChar;
        if (string.Equals(output, freeze, Comparison) || string.Equals(output, sourceRoot, Comparison) || output.StartsWith(prefix, Comparison))
            throw new InvalidOperationException("Dry-run output must not overlap source inputs.");
        if (File.Exists(output) || Directory.Exists(output))
            throw new InvalidOperationException("Dry-run output already exists; select a new path.");
        for (var directory = new DirectoryInfo(Path.GetDirectoryName(output)!); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, ".git")) || Directory.Exists(Path.Combine(directory.FullName, ".git")))
                throw new InvalidOperationException("Dry-run reports must be outside Git checkouts.");
            if (directory.Exists && (directory.Attributes & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("Dry-run output must not traverse linked directories.");
        }
        return output;
    }

    private static StringComparison Comparison => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
