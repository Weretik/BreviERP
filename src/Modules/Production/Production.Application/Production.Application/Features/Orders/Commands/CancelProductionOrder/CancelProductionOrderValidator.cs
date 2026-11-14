using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.CancelProductionOrder;

public sealed class CancelProductionOrderValidator : AbstractValidator<CancelProductionOrderCommand>
{
    public CancelProductionOrderValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
        RuleFor(x => x.Input.Reason).NotEmpty().WithMessage("Вкажіть причину скасування.")
            .MaximumLength(ProductionOrder.CancelReasonMaxLength).When(x => x.Input is not null);
    }
}
