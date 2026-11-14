using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.EditFabricMovement;

public sealed class EditFabricMovementValidator : AbstractValidator<EditFabricMovementCommand>
{
    public EditFabricMovementValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.MovementId).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.ExpectedRevision).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Input.OperationDate).OperationDate();
            RuleFor(x => x.Input.Meters).Must(FabricStock.IsValidMeters).WithMessage(StockMessages.Meters);
            RuleFor(x => x.Input.FabricCode).MaximumLength(FabricStock.CodeMaxLength);
            RuleFor(x => x.Input.Color).MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.SupplierName).FormText();
            RuleFor(x => x.Input.Document).FormText();
            RuleFor(x => x.Input.Destination).FormText();
            RuleFor(x => x.Input.Reason).MaximumLength(FabricMovementRevision.ReasonMaxLength);
        });
    }
}
