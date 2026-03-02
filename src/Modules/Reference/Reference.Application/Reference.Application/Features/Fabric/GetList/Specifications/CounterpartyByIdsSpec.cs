using Crm.Domain.Entities;
using Crm.Domain.ValueObjects;

namespace Reference.Application.GetAllFabric.Specifications;

public sealed class CounterpartyByIdsSpec : Specification<Counterparty>
{
    public CounterpartyByIdsSpec(IEnumerable<CounterpartyId> ids)
    {
        var idList = ids.ToList();

        Query.AsNoTracking()
            .Where(c => idList.Contains(c.Id));
    }
}
