using Application.CRM.GetCounterparty.DTOs;
using Domain.CRM.Entities;

namespace Application.CRM.GetCounterparty.Specifications;

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
