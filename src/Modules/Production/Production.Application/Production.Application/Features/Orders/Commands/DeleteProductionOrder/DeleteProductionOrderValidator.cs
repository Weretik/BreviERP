using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Commands.DeleteProductionOrder;

public sealed class DeleteProductionOrderValidator : AbstractValidator<DeleteProductionOrderCommand>
{
    public DeleteProductionOrderValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Version).GreaterThan(0);
    }
}
