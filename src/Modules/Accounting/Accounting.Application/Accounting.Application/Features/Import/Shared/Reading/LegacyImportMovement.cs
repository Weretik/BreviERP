using System.Text.Json.Serialization;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Application.Features.Import.Shared.Reading;

[JsonConverter(typeof(JsonStringEnumConverter<LegacyImportMovement>))]
public enum LegacyImportMovement
{
    Income,
    Expense,
    Transfer,
}
