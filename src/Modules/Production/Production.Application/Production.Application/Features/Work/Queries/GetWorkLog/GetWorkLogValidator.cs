using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Queries.GetWorkLog;

public sealed class GetWorkLogValidator : AbstractValidator<GetWorkLogQuery>
{
    public GetWorkLogValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => WorkMonth.TryParse(x, out _)).WithMessage(WorkMessages.Month);
        RuleFor(x => x.EmployeeId).GreaterThan(0).When(x => x.EmployeeId is not null);
        RuleFor(x => x.OrderId).GreaterThan(0).When(x => x.OrderId is not null);
    }
}
