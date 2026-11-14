using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;

namespace Warehouse.Application.Features.Stock.Commands.ReceiveFabric;

public sealed class ReceiveFabricValidator : AbstractValidator<ReceiveFabricCommand>
{
    public ReceiveFabricValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.FabricId).GreaterThan(0).WithMessage("Оберіть тканину.");
            RuleFor(x => x.Input.Color).NotEmpty().WithMessage("Оберіть колір.").MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.Meters).PositiveMeters();
            RuleFor(x => x.Input.OperationDate).OperationDate();
            RuleFor(x => x.Input.SupplierName).FormText();
            RuleFor(x => x.Input.Document).FormText();
            RuleFor(x => x.Input.Note).MaximumLength(FabricMovement.NoteMaxLength);
        });
    }
}
