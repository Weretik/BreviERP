using System.Text.Json.Serialization;

namespace Accounting.Domain.Counterparties;

/// <summary>
/// Who an operation is with: a finance counterparty of this module, a client of «Клієнти» (Crm) or a supplier of
/// «Постачальники» (Reference). Clients and suppliers are linked by their id only, without a database link.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<CounterpartyKind>))]
public enum CounterpartyKind
{
    Financial,
    Client,
    Supplier,
}
