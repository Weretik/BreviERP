namespace Crm.Application.Quotes.Pdf;

/// <summary>Client texts as the PDF prints them.</summary>
public static class QuotePdfText
{
    /// <summary>
    /// Joins a line to the one before it when it goes on with a small letter: descriptions copied from documents
    /// keep the line breaks of the narrow column they came from («кишені у\nбічних швах»). Paragraphs and lines
    /// that start a new sentence or a list item stay as they are.
    /// </summary>
    public static string? Unwrap(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var result = new System.Text.StringBuilder(lines[0].TrimEnd());
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var previous = lines[i - 1].Trim();
            var continues = line.Length > 0 && previous.Length > 0 && char.IsLower(line[0]);
            result.Append(continues ? ' ' : '\n').Append(line);
        }

        return result.ToString();
    }
}
