using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Queries.GetWorkChoices;

public sealed class GetWorkChoicesValidator : AbstractValidator<GetWorkChoicesQuery>
{
    public GetWorkChoicesValidator() => RuleFor(x => x.Actor).ValidActor();
}
