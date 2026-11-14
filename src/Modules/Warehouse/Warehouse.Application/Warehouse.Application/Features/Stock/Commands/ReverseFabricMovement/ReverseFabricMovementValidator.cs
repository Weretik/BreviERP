using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;

public sealed class ReverseFabricMovementValidator : AbstractValidator<ReverseFabricMovementCommand>
{
    public ReverseFabricMovementValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.MovementId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () => RuleFor(x => x.Input.Reason).RevisionReason());
    }
}
