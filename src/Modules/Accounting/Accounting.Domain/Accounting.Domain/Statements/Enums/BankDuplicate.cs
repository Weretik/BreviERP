using System.Text.Json.Serialization;

namespace Accounting.Domain.Statements.Enums;

/// <summary>
/// Whether the line is already in the books: <see cref="Exact"/> — the same bank id or fingerprint, never booked;
/// <see cref="Probable"/> — a similar record, excluded at first but the director may include it.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<BankDuplicate>))]
public enum BankDuplicate
{
    None,
    Probable,
    Exact,
}
