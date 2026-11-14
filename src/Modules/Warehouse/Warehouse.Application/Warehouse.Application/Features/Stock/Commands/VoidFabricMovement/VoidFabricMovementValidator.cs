using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.VoidFabricMovement;

public sealed class VoidFabricMovementValidator : AbstractValidator<VoidFabricMovementCommand>
{
    public VoidFabricMovementValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.MovementId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.ExpectedRevision).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Input.Reason).RevisionReason();
        });
    }
}
