using Accounting.Domain.Counterparties;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Statements.Shared;

/// <summary>
/// Who an operation is with. <c>Id</c> is text: a UUID for a finance counterparty and a client, a number for a
/// supplier. <c>Missing</c> is true when the record is gone from its list (the link stays).
/// </summary>
public sealed record FinanceCounterpartyRefResponse(CounterpartyKind Kind, string Id, string Name, bool Active,
    bool Verified, bool Missing)
{
    internal static FinanceCounterpartyRefResponse? From(CounterpartyLink? link, FinanceNames names)
    {
        if (link is null) return null;
        var state = names.Counterparties.Of(link);
        return state is null
            ? new FinanceCounterpartyRefResponse(link.Kind, link.Id, string.Empty, false, true, true)
            : new FinanceCounterpartyRefResponse(link.Kind, link.Id, state.Name, state.Active, state.Verified, false);
    }
}
