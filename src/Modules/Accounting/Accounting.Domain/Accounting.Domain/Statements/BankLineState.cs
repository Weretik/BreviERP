using System.Text.Json.Serialization;

namespace Accounting.Domain.Statements;

/// <summary>
/// What a statement line needs, worst first: <see cref="Error"/> (never booked), <see cref="Duplicate"/> (already in the
/// books), <see cref="Transfer"/> (to or from another own account), <see cref="NewCounterparty"/> (nobody recognised, or
/// an unconfirmed finance counterparty), <see cref="NeedsCategory"/> and <see cref="Ready"/>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<BankLineState>))]
public enum BankLineState
{
    Ready,
    NeedsCategory,
    NewCounterparty,
    Transfer,
    Duplicate,
    Error,
}
