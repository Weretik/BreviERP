using Application.CRM.GetCounterparty.DTOs;

namespace Application.CRM.GetCounterparty;

public sealed record GetCounterpartyQuery : IQuery<Result<List<CounterpartyDto>>> { }
