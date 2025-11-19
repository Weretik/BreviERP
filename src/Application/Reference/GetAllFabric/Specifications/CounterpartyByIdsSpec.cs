using Domain.CRM.Entities;
using Domain.CRM.ValueObjects;

namespace Application.Reference.GetAllFabric;

public sealed class CounterpartyByIdsSpec : Specification<Counterparty>
{
    public CounterpartyByIdsSpec(IEnumerable<CounterpartyId> ids)
    {
        var idList = ids.ToList();

        Query.AsNoTracking()
            .Where(c => idList.Contains(c.Id));
    }
}
