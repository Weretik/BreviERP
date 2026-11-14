using System.Text;

namespace Catalog.Infrastructure.Integrations.PpeSupplierSites.Pages;

/// <summary>Splits CSV text into rows of cells: commas, quoted cells with doubled quotes and line breaks inside.</summary>
internal static class CsvRows
{
    public static IEnumerable<IReadOnlyList<string>> Parse(string csv)
    {
        var row = new List<string>();
        var cell = new StringBuilder();
        var quoted = false;
        for (var index = 0; index < csv.Length; index++)
        {
            var character = csv[index];
            if (quoted)
            {
                if (character != '"') cell.Append(character);
                else if (index + 1 < csv.Length && csv[index + 1] == '"') { cell.Append('"'); index++; }
                else quoted = false;
                continue;
            }

            switch (character)
            {
                case '"': quoted = true; break;
                case ',': row.Add(cell.ToString()); cell.Clear(); break;
                case '\r': break;
                case '\n':
                    row.Add(cell.ToString());
                    cell.Clear();
                    yield return row;
                    row = [];
                    break;
                default: cell.Append(character); break;
            }
        }

        if (cell.Length > 0 || row.Count > 0)
        {
            row.Add(cell.ToString());
            yield return row;
        }
    }
}
