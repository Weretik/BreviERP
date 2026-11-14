using System.Text.Json.Serialization;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties.Results;

/// <summary>Tabs of «Контрагенти»: «Фінансові», «Клієнти», «Постачальники», «Разові».</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FinanceCounterpartyTab>))]
public enum FinanceCounterpartyTab
{
    Financial,
    Clients,
    Suppliers,
    Oneoffs,
}
