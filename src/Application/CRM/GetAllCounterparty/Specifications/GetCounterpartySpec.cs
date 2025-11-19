using Domain.CRM.Entities;

namespace Application.CRM.GetAllCounterparty;

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
