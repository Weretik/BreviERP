using System.Text.RegularExpressions;

namespace CatalogImport.Source;

public static partial class CatalogSourceIdentity
{
    public static string ModelCode(WorkbookRow article) => ModelPattern().Match(article.Text(6)).Value;

    public static string OperationKey(WorkbookRow row) =>
        row.Text(5).Length == 0 ? $"MANUAL:{row.Key}" : row.Text(5);

    [GeneratedRegex(@"MOD-\d+")]
    private static partial Regex ModelPattern();
}

