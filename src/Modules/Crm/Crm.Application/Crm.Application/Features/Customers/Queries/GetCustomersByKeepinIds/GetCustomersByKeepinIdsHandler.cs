using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using Crm.Application.Abstractions.Customers;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByKeepinIds;

public sealed class GetCustomersByKeepinIdsHandler(ICustomerReadRepository repository)
    : IQueryHandler<GetCustomersByKeepinIdsQuery, IReadOnlyList<KeepinCustomerLookup>>
{
    public async ValueTask<IReadOnlyList<KeepinCustomerLookup>> Handle(GetCustomersByKeepinIdsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.KeepinIds.Count == 0) return [];
        var wanted = query.KeepinIds.Distinct().ToList();
        var rows = await repository.ListAsync(new CustomersByKeepinIdsSpec(wanted), cancellationToken);
        // A merged customer may hold several KeepinCRM ids; each wanted id points to one customer.
        return rows
            .SelectMany(x => x.KeepinIds.Where(wanted.Contains)
                .Select(id => new KeepinCustomerLookup(id, x.PublicId, x.DisplayName, x.TaxIdentity, x.Active)))
            .GroupBy(x => x.KeepinId).Select(x => x.First()).ToList();
    }
}
