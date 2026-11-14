using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock.Movements;

namespace Warehouse.Application.Features.Stock.Commands.IssueFabric;

public sealed class IssueFabricValidator : AbstractValidator<IssueFabricCommand>
{
    public IssueFabricValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.FabricCode).NotEmpty().WithMessage("Оберіть тканину зі складу.")
                .MaximumLength(FabricStock.CodeMaxLength);
            RuleFor(x => x.Input.Color).NotEmpty().WithMessage("Оберіть колір зі складу.")
                .MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.ExpectedVersion).MaximumLength(64);
            RuleFor(x => x.Input.Meters).PositiveMeters();
            RuleFor(x => x.Input.OperationDate).OperationDate();
            RuleFor(x => x.Input.Destination).FormText();
            RuleFor(x => x.Input.Note).MaximumLength(FabricMovement.NoteMaxLength);
        });
    }
}
