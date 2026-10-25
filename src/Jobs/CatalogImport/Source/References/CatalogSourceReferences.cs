namespace CatalogImport.Source;

public static class CatalogSourceReferences
{
    public static IReadOnlyList<WorkbookRow> Fabrics(WorkbookSnapshot source) =>
        source.Rows("Тканини").Where(row =>
            !(row.Text(1).Length == 0 && row.Text(5).Length == 0 && !row.Flag(7))).ToArray();
}

