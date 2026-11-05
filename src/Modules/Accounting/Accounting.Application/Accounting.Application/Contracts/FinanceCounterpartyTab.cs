using System.Text.Json.Serialization;

namespace Accounting.Application.Contracts;

/// <summary>Tabs of «Контрагенти»: «Фінансові», «Клієнти», «Постачальники», «Разові».</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FinanceCounterpartyTab>))]
public enum FinanceCounterpartyTab
{
    Financial,
    Clients,
    Suppliers,
    Oneoffs,
}
