using Accounting.Application.Abstractions;

namespace UnitTests.Accounting;

/// <summary>Clients and suppliers of other modules, kept in memory for the Accounting tests.</summary>
internal sealed class FakeAccountingParties : IAccountingCustomerReader, IAccountingSupplierReader
{
    public List<AccountingParty> Clients { get; } = [];
    public List<AccountingParty> Suppliers { get; } = [];

    public Task<AccountingPartyPage> SearchAsync(string? search, bool activeOnly, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var rows = Clients.Where(x => !activeOnly || x.Active)
            .Where(x => search is null || x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult(new AccountingPartyPage(rows.Skip((page - 1) * pageSize).Take(pageSize).ToList(), rows.Count));
    }

    public Task<IReadOnlyList<AccountingParty>> FindAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AccountingParty>>(
            Clients.Where(x => ids.Contains(Guid.Parse(x.Id))).ToList());

    public Task<IReadOnlyList<AccountingParty>> FindByKeepinIdsAsync(IReadOnlyCollection<string> keepinIds,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AccountingParty>>(
            Clients.Where(x => x.ExternalId is not null && keepinIds.Contains(x.ExternalId)).ToList());

    public Task<IReadOnlyList<AccountingParty>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AccountingParty>>(Suppliers);
}
