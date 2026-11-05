using System.Text.Json.Serialization;

namespace Accounting.Application.Journal;

/// <summary>Sort key of «Операції» and «Перекази»; equal keys fall back to the newest record first.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FinanceJournalSort>))]
public enum FinanceJournalSort
{
    Date,
    Amount,
}

[JsonConverter(typeof(JsonStringEnumConverter<FinanceSortDirection>))]
public enum FinanceSortDirection
{
    Desc,
    Asc,
}

public static class FinanceJournal
{
    /// <summary>Fixed page size; a client cannot change it.</summary>
    public const int PageSize = 50;

    public const int SearchMaxLength = 200;
}
