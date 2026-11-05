using System.Text.Json.Serialization;

namespace Accounting.Domain.Ledger;

/// <summary>
/// Area of the books: «Домашня бухгалтерія» (archive before <see cref="LedgerCalendar.CutoverDate"/>) or the current
/// books from that date on. Categories belong to one area; accounts serve both.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<LedgerScope>))]
public enum LedgerScope
{
    Current,
    LegacyArchive,
}
