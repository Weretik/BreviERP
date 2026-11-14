using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.CopyProductionOrder;

public sealed class CopyProductionOrderValidator : AbstractValidator<CopyProductionOrderCommand>
{
    public CopyProductionOrderValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.RequestId).NotEmpty().When(x => x.Input is not null);
    }
}
