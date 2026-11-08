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

    /// <summary>
    /// The clients linked to these KeepinCRM client ids (archived too), with the KeepinCRM id in
    /// <see cref="AccountingParty.ExternalId"/>; used when the old finance archive is moved over (stage 5).
    /// </summary>
    Task<IReadOnlyList<AccountingParty>> FindByKeepinIdsAsync(IReadOnlyCollection<string> keepinIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Active clients whose ЄДРПОУ/ІПН is one of <paramref name="taxIds"/>; the statement import (stage 6) recognises a
    /// payer by it.
    /// </summary>
    Task<IReadOnlyList<AccountingParty>> FindByTaxIdsAsync(IReadOnlyCollection<string> taxIds,
        CancellationToken cancellationToken);
}

/// <summary>«Постачальники» of the Reference module for operations; implemented in the host.</summary>
public interface IAccountingSupplierReader
{
    /// <summary>All suppliers, inactive too (the list is short).</summary>
    Task<IReadOnlyList<AccountingParty>> ListAsync(CancellationToken cancellationToken);
}

/// <summary>
/// A client or supplier as «Бухгалтерія» shows it; <c>Id</c> is its id as text, <c>TaxId</c> its ЄДРПОУ/ІПН and
/// <c>ExternalId</c> its KeepinCRM id when known.
/// </summary>
public sealed record AccountingParty(string Id, string Name, string TaxId, bool Active, string? ExternalId = null);

public sealed record AccountingPartyPage(IReadOnlyList<AccountingParty> Items, int Total);
