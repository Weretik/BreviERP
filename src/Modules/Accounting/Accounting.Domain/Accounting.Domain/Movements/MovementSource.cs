using System.Text.Json.Serialization;

namespace Accounting.Domain.Movements;

/// <summary>
/// Where a movement came from: typed by hand, a bank statement (stage 6) or the one-off move of the old Calculator
/// «Домашня бухгалтерія» (stage 5). Only <see cref="Manual"/> records are created by the forms.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MovementSource>))]
public enum MovementSource
{
    Manual,
    BankImport,
    HomeAccountingImport,
}
