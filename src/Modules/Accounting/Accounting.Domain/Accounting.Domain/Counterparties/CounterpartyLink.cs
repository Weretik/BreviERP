using Accounting.Domain;

namespace Accounting.Domain.Counterparties;

/// <summary>
/// The counterparty chosen for an operation: its kind and id as text — a UUID for a finance counterparty and a client,
/// a positive number for a supplier. The name is never the identity.
/// </summary>
public sealed record CounterpartyLink(CounterpartyKind Kind, string Id)
{
    public const int IdMaxLength = 64;

    public static DomainResult<CounterpartyLink> Of(CounterpartyKind kind, string? id)
    {
        var clean = (id ?? string.Empty).Trim();
        var valid = kind switch
        {
            CounterpartyKind.Financial or CounterpartyKind.Client =>
                Guid.TryParse(clean, out var guid) && guid != Guid.Empty ? guid.ToString("D") : null,
            CounterpartyKind.Supplier => int.TryParse(clean, out var number) && number > 0 ? number.ToString() : null,
            _ => null,
        };
        return valid is null
            ? AccountingErrors.Rule("Accounting.Counterparty.Link", "Контрагента не знайдено. Оберіть його зі списку.")
            : new CounterpartyLink(kind, valid);
    }

    public static CounterpartyLink Of(FinanceCounterpartyId id) => new(CounterpartyKind.Financial, id.Value.ToString("D"));
}

/// <summary>What the module knows about a linked counterparty when it checks an operation.</summary>
public sealed record CounterpartyState(CounterpartyLink Link, string Name, bool Active, bool Verified);
