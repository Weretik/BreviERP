using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByKeepinIds;

/// <summary>
/// Clients linked to these KeepinCRM client ids, archived too, merged ones never; for another module that moves old
/// records keyed by KeepinCRM id (for example the finance archive of «Бухгалтерія»).
/// </summary>
public sealed record GetCustomersByKeepinIdsQuery(IReadOnlyCollection<string> KeepinIds)
    : IQuery<IReadOnlyList<KeepinCustomerLookup>>;
