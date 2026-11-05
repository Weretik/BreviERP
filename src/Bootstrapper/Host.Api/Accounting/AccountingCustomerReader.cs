using Accounting.Application.Abstractions;
using Crm.Application.Customers.Lookup;
using Mediator;

namespace Host.Api.Accounting;

/// <summary>«Клієнти» (Crm) for the operations of «Бухгалтерія»; every client, since the module is the director's.</summary>
internal sealed class AccountingCustomerReader(ISender sender) : IAccountingCustomerReader
{
    public async Task<AccountingPartyPage> SearchAsync(string? search, bool activeOnly, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerLookupQuery(search, null, activeOnly, page, pageSize), cancellationToken);
        return new AccountingPartyPage(result.Items.Select(Party).ToArray(), result.Total);
    }

    public async Task<IReadOnlyList<AccountingParty>> FindAsync(IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return [];
        var result = await sender.Send(new GetCustomerLookupQuery(null, ids, false, 1, Math.Max(ids.Count, 1)),
            cancellationToken);
        return result.Items.Select(Party).ToArray();
    }

    private static AccountingParty Party(CustomerLookup x) =>
        new(x.PublicId.ToString("D"), x.DisplayName, x.TaxIdentity ?? string.Empty, x.Active);
}
