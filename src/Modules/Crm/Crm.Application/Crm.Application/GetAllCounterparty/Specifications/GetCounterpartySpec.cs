using Application.CRM.GetAllCounterparty;
using Crm.Domain.Entities;

namespace Crm.Application.GetAllCounterparty.Specifications;

public class GetCounterpartySpec : Specification<Counterparty, CounterpartyDto>
{
    public GetCounterpartySpec()
    {
        Query.AsNoTracking();

        Query.Select(c =>
            new CounterpartyDto(
                c.Id.Value,
                c.Name,
                c.Type.Name)
        );
    }
}
