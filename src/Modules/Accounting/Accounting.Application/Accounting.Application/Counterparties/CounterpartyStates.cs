using Accounting.Application.Abstractions;
using Accounting.Domain.Counterparties;

namespace Accounting.Application.Counterparties;

/// <summary>
/// Names and states of the counterparties linked to some operations: finance ones from this module, clients and
/// suppliers through their readers. A link whose record is gone has no state.
/// </summary>
internal sealed class CounterpartyStates(IReadOnlyDictionary<CounterpartyLink, CounterpartyState> states)
{
    public static readonly CounterpartyStates Empty = new(new Dictionary<CounterpartyLink, CounterpartyState>());

    public CounterpartyState? Of(CounterpartyLink? link) =>
        link is not null && states.TryGetValue(link, out var state) ? state : null;

    public static async Task<CounterpartyStates> LoadAsync(CounterpartyReaders readers,
        IEnumerable<CounterpartyLink?> links, CancellationToken cancellationToken)
    {
        var wanted = links.OfType<CounterpartyLink>().Distinct().ToList();
        var states = new Dictionary<CounterpartyLink, CounterpartyState>();
        if (wanted.Count == 0) return new CounterpartyStates(states);

        var financial = wanted.Where(x => x.Kind == CounterpartyKind.Financial)
            .Select(x => FinanceCounterpartyId.From(Guid.Parse(x.Id))).ToList();
        if (financial.Count > 0)
            foreach (var counterparty in await readers.Db.Counterparties.AsNoTracking()
                         .Where(x => financial.Contains(x.Id)).ToListAsync(cancellationToken))
                states[counterparty.Link] = counterparty.State;

        var clients = wanted.Where(x => x.Kind == CounterpartyKind.Client).Select(x => Guid.Parse(x.Id)).ToList();
        if (clients.Count > 0)
            foreach (var client in await readers.Customers.FindAsync(clients, cancellationToken))
                Add(states, CounterpartyKind.Client, client);

        if (wanted.Any(x => x.Kind == CounterpartyKind.Supplier))
        {
            var suppliers = wanted.Where(x => x.Kind == CounterpartyKind.Supplier).Select(x => x.Id).ToHashSet();
            foreach (var supplier in (await readers.Suppliers.ListAsync(cancellationToken)).Where(x => suppliers.Contains(x.Id)))
                Add(states, CounterpartyKind.Supplier, supplier);
        }

        return new CounterpartyStates(states);
    }

    /// <summary>The state of one chosen counterparty; null with a message when it is gone.</summary>
    public static async Task<(CounterpartyState? State, string? Missing)> ResolveAsync(CounterpartyReaders readers,
        CounterpartyLink? link, CancellationToken cancellationToken)
    {
        if (link is null) return (null, null);
        var state = (await LoadAsync(readers, [link], cancellationToken)).Of(link);
        return state is null ? (null, AccountingMessages.CounterpartyNotFound) : (state, null);
    }

    // Clients and suppliers are checked records of their own modules, so they never need a review here.
    private static void Add(Dictionary<CounterpartyLink, CounterpartyState> states, CounterpartyKind kind, AccountingParty party)
    {
        var link = CounterpartyLink.Of(kind, party.Id);
        states[link] = new CounterpartyState(link, party.Name, party.Active, Verified: true);
    }
}

/// <summary>Where counterparties are read from: this module and the client and supplier readers of the host.</summary>
internal sealed record CounterpartyReaders(IAccountingDbContext Db, IAccountingCustomerReader Customers,
    IAccountingSupplierReader Suppliers);
