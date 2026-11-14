using Crm.Domain.Customers;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients.Results;

namespace Crm.Application.Features.Customers.Queries.GetCustomersByKeepinIds;

internal sealed class CustomersByKeepinIdsSpec : Specification<Customer, KeepinCustomerRow>
{
    public CustomersByKeepinIdsSpec(List<string> keepinIds)
    {
        Query.AsNoTracking()
            .Where(x => x.MergedIntoId == null && x.ExternalIdentities.Any(i => i.Source == KeepinCrmClientIdentity.Source &&
                i.Type == KeepinCrmClientIdentity.Type && keepinIds.Contains(i.ExternalId)));
        Query.Select(x => new KeepinCustomerRow(x.PublicId, x.DisplayName, x.TaxIdentity, x.Active,
            x.ExternalIdentities
                .Where(i => i.Source == KeepinCrmClientIdentity.Source && i.Type == KeepinCrmClientIdentity.Type)
                .Select(i => i.ExternalId).ToList()));
    }
}
