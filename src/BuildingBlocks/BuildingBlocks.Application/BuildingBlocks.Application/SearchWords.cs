using System.Linq.Expressions;
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
        string.IsNullOrWhiteSpace(input)
            ? []
            : input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => $"%{EscapeWildcards(Normalize(word))}%").Distinct().ToList();

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
