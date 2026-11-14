using Accounting.Application.Abstractions;
using Crm.Contracts.Customers;

namespace Accounting.Infrastructure.Adapters.Crm;

/// <summary>«Клієнти» (Crm) for the operations of «Бухгалтерія»; every client, since the module is the director's.</summary>
internal sealed class AccountingCustomerReader(ICustomerDirectory customers) : IAccountingCustomerReader
{
    public async Task<AccountingPartyPage> SearchAsync(string? search, bool activeOnly, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await customers.SearchAsync(search, null, activeOnly, page, pageSize, cancellationToken);
        return new AccountingPartyPage(result.Items.Select(Party).ToArray(), result.Total);
    }

    public async Task<IReadOnlyList<AccountingParty>> FindAsync(IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return [];
        var result = await customers.SearchAsync(null, ids, false, 1, Math.Max(ids.Count, 1), cancellationToken);
        return result.Items.Select(Party).ToArray();
    }

    public async Task<IReadOnlyList<AccountingParty>> FindByKeepinIdsAsync(IReadOnlyCollection<string> keepinIds,
        CancellationToken cancellationToken) =>
        (await customers.FindByKeepinIdsAsync(keepinIds, cancellationToken))
        .Select(x => new AccountingParty(x.PublicId.ToString("D"), x.DisplayName, x.TaxIdentity ?? string.Empty,
            x.Active, x.KeepinId)).ToArray();

    public async Task<IReadOnlyList<AccountingParty>> FindByTaxIdsAsync(IReadOnlyCollection<string> taxIds,
        CancellationToken cancellationToken) =>
        (await customers.FindByTaxIdsAsync(taxIds, cancellationToken)).Select(Party).ToArray();

    private static AccountingParty Party(CustomerInfo x) =>
        new(x.PublicId.ToString("D"), x.DisplayName, x.TaxIdentity ?? string.Empty, x.Active);
}
