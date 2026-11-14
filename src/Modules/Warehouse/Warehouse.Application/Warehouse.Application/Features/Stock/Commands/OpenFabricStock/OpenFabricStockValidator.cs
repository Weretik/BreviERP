using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.OpenFabricStock;

public sealed class OpenFabricStockValidator : AbstractValidator<OpenFabricStockCommand>
{
    public OpenFabricStockValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.FabricId).GreaterThan(0).WithMessage("Оберіть тканину.");
            RuleFor(x => x.Input.Color).NotEmpty().WithMessage("Оберіть колір.").MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.Meters).Must(x => x >= 0 && FabricStock.IsValidMeters(x))
                .WithMessage("Початковий залишок — від 0 до 1 000 000 м, до сотих.");
            RuleFor(x => x.Input.OperationDate).OperationDate();
        });
    }
}
