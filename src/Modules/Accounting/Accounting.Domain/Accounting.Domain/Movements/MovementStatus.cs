using System.Text.Json.Serialization;

namespace Accounting.Domain.Movements;

/// <summary>A movement counts in balances and totals while <see cref="Active"/>; «Анулювати» makes it <see cref="Void"/>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MovementStatus>))]
public enum MovementStatus
{
    Active,
    Void,
}
