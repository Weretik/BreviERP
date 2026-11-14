using System.Text.Json.Serialization;

namespace Accounting.Domain.Statements.Enums;

/// <summary>A statement is checked and corrected while <see cref="Preview"/>; «Провести виписку» makes it <see cref="Booked"/>.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<BankStatementStatus>))]
public enum BankStatementStatus
{
    Preview,
    Booked,
}
