using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application;

/// <summary>
/// The text of a search box as words (database rules 2.1): every word must match, each as a part of the lower-case
/// <c>SearchText</c> column that <c>SearchTextColumn</c> maintains and indexes with <c>gin_trgm_ops</c>.
/// </summary>
public static class SearchWords
{
    /// <summary>The escape character of the <see cref="Patterns"/>.</summary>
    public const string Escape = "\\";

    /// <summary>The name of the generated search column.</summary>
    public const string Column = "SearchText";

    /// <summary>One <c>LIKE</c> pattern per distinct word: lower case, apostrophes unified, wildcards escaped.</summary>
    public static IReadOnlyList<string> Patterns(string? input) =>
        Words(input).Select(word => $"%{EscapeWildcards(word)}%").ToList();

    /// <summary>The distinct words of the input, normalized as the generated column stores text.</summary>
    public static IReadOnlyList<string> Words(string? input) =>
        string.IsNullOrWhiteSpace(input)
            ? []
            : input.Normalize(NormalizationForm.FormC).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Select(Normalize).Distinct().ToList();

    /// <summary>
    /// The same rule in memory, for a short list that is read whole anyway: every word is a part of one of the
    /// <paramref name="texts"/>, in any order.
    /// </summary>
    public static bool MatchesEveryWord(string? input, IEnumerable<string?> texts)
    {
        var words = Words(input);
        if (words.Count == 0) return true;
        var text = Normalize(string.Join('\n', texts.OfType<string>()).Normalize(NormalizationForm.FormC));
        return words.All(word => text.Contains(word, StringComparison.Ordinal));
    }

    /// <summary>Lower case with one apostrophe, the same as the generated column stores.</summary>
    public static string Normalize(string text) =>
        text.ToLowerInvariant().Replace('’', '\'').Replace('ʼ', '\'');

    /// <summary>Keeps the rows where every word matches; <paramref name="match"/> gets one pattern.</summary>
    public static IQueryable<T> WhereEveryWord<T>(this IQueryable<T> rows, string? input,
        Func<string, Expression<Func<T, bool>>> match)
    {
        foreach (var pattern in Patterns(input)) rows = rows.Where(match(pattern));
        return rows;
    }

    /// <summary>Keeps the rows whose generated <c>SearchText</c> holds every word.</summary>
    public static IQueryable<T> WhereSearchText<T>(this IQueryable<T> rows, string? input) where T : class =>
        rows.WhereEveryWord(input, Matches<T>);

    /// <summary>The generated <c>SearchText</c> of the row holds <paramref name="pattern"/>.</summary>
    public static Expression<Func<T, bool>> Matches<T>(string pattern) where T : class =>
        x => EF.Functions.Like(EF.Property<string>(x, Column), pattern, Escape);

    private static string EscapeWildcards(string word) =>
        word.Replace(Escape, Escape + Escape).Replace("%", Escape + "%").Replace("_", Escape + "_");
}
