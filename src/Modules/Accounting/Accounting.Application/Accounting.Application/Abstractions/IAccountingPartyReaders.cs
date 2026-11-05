namespace Accounting.Application.Abstractions;

/// <summary>
/// «Клієнти» of the Crm module for operations; implemented in the host. «Бухгалтерія» is open to the director only, so
/// every client is visible here regardless of its manager.
/// </summary>
public interface IAccountingCustomerReader
{
    /// <summary>Clients whose name, ЄДРПОУ or contact matches <paramref name="search"/>; merged ones never come.</summary>
    Task<AccountingPartyPage> SearchAsync(string? search, bool activeOnly, int page, int pageSize,
        CancellationToken cancellationToken);

    /// <summary>The clients with these ids, archived too, so an old link still shows its name.</summary>
    Task<IReadOnlyList<AccountingParty>> FindAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}

/// <summary>«Постачальники» of the Reference module for operations; implemented in the host.</summary>
public interface IAccountingSupplierReader
{
    /// <summary>All suppliers, inactive too (the list is short).</summary>
    Task<IReadOnlyList<AccountingParty>> ListAsync(CancellationToken cancellationToken);
}

/// <summary>A client or supplier as «Бухгалтерія» shows it; <c>Id</c> is its id as text, <c>TaxId</c> its ЄДРПОУ/ІПН.</summary>
public sealed record AccountingParty(string Id, string Name, string TaxId, bool Active);

public sealed record AccountingPartyPage(IReadOnlyList<AccountingParty> Items, int Total);
