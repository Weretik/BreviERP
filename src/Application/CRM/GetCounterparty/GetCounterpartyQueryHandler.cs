using Application.CRM.GetCounterparty.DTOs;
using Application.CRM.GetCounterparty.Specifications;
using Application.CRM.Shared.Contracts;
using Domain.CRM.Entities;

namespace Application.CRM.GetCounterparty;

public class GetCounterpartyQueryHandler(ICounterpartyReadRepository<Counterparty> repository)
    : IQueryHandler<GetCounterpartyQuery, Result<List<CounterpartyDto>>>
{
    public async ValueTask<Result<List<CounterpartyDto>>> Handle(
        GetCounterpartyQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var spec = new GetCounterpartySpec();
        var items = await repository.ListAsync(spec, cancellationToken);
        if (items.Count == 0) return Result.NotFound();

        return Result.Success(items);
    }
}
