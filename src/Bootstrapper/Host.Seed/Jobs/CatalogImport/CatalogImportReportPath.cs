namespace Host.Seed.Jobs;

internal static class CatalogImportReportPath
{
    public static string Resolve(string path, string? execution)
    {
        const string placeholder = "{execution}";
        if (!path.Contains(placeholder, StringComparison.Ordinal))
            return path;
        if (execution is not { Length: > 0 and <= 128 }
            || execution.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-'))
            throw new InvalidOperationException("Catalogue report placeholder requires a valid CLOUD_RUN_EXECUTION.");
        return path.Replace(placeholder, execution, StringComparison.Ordinal);
    }
}
