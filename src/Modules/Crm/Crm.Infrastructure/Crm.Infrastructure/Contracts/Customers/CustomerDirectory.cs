using Crm.Contracts.Customers;
using Mediator;
using Crm.Application.Features.Customers.Queries.GetCustomerLookup;
using Crm.Application.Features.Customers.Queries.GetCustomersByKeepinIds;
using Crm.Application.Features.Customers.Queries.GetCustomersByTaxIds;

namespace Crm.Infrastructure.Contracts.Customers;

/// <summary>«Клієнти» for other modules through the Crm lookup queries.</summary>
internal sealed class CustomerDirectory(ISender sender) : ICustomerDirectory
{
    public async Task<CustomerPage> SearchAsync(string? search, IReadOnlyCollection<Guid>? ids, bool activeOnly,
        int page, int pageSize, CancellationToken cancellationToken)
    {
        if (ids is { Count: 0 }) return new CustomerPage([], 0);
        var result = await sender.Send(new GetCustomerLookupQuery(search, ids, activeOnly, page, pageSize),
            cancellationToken);
        return new CustomerPage(result.Items.Select(Info).ToArray(), result.Total);
    }

    public async Task<IReadOnlyList<CustomerInfo>> FindByKeepinIdsAsync(IReadOnlyCollection<string> keepinIds,
        CancellationToken cancellationToken)
    {
        if (keepinIds.Count == 0) return [];
        var rows = await sender.Send(new GetCustomersByKeepinIdsQuery(keepinIds), cancellationToken);
        return rows.Select(x => new CustomerInfo(x.PublicId, x.DisplayName, x.TaxIdentity, x.Active, x.KeepinId))
            .ToArray();
    }

    public async Task<IReadOnlyList<CustomerInfo>> FindByTaxIdsAsync(IReadOnlyCollection<string> taxIds,
        CancellationToken cancellationToken)
    {
        if (taxIds.Count == 0) return [];
        return (await sender.Send(new GetCustomersByTaxIdsQuery(taxIds), cancellationToken)).Select(Info).ToArray();
    }

    private static CustomerInfo Info(CustomerLookup x) => new(x.PublicId, x.DisplayName, x.TaxIdentity, x.Active);
}
