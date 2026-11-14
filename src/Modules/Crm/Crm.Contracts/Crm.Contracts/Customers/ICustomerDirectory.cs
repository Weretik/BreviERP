namespace Crm.Contracts.Customers;

/// <summary>A client of «Клієнти»; <paramref name="KeepinId"/> is set only when asked by KeepinCRM id.</summary>
public sealed record CustomerInfo(Guid PublicId, string DisplayName, string? TaxIdentity, bool Active,
    string? KeepinId = null);

public sealed record CustomerPage(IReadOnlyList<CustomerInfo> Items, int Total);

/// <summary>Clients of «Клієнти» for other modules; every client, without the manager's access rule.</summary>
public interface ICustomerDirectory
{
    /// <summary>A page of clients by name or ЄДРПОУ, or the clients with <paramref name="ids"/>.</summary>
    Task<CustomerPage> SearchAsync(string? search, IReadOnlyCollection<Guid>? ids, bool activeOnly, int page,
        int pageSize, CancellationToken cancellationToken);

    Task<IReadOnlyList<CustomerInfo>> FindByKeepinIdsAsync(IReadOnlyCollection<string> keepinIds,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CustomerInfo>> FindByTaxIdsAsync(IReadOnlyCollection<string> taxIds,
        CancellationToken cancellationToken);
}
