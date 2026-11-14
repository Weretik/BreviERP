using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.AdjustFabricStock;

public sealed class AdjustFabricStockValidator : AbstractValidator<AdjustFabricStockCommand>
{
    public const int ReasonMinLength = 3;
    public const int ReasonMaxLength = 500;

    public AdjustFabricStockValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.OperationId).OperationId();
            RuleFor(x => x.Input.FabricCode).NotEmpty().MaximumLength(FabricStock.CodeMaxLength);
            RuleFor(x => x.Input.Color).NotEmpty().MaximumLength(StockColor.MaxLength);
            RuleFor(x => x.Input.ExpectedVersion).NotEmpty().WithMessage(StockMessages.Stale).MaximumLength(64);
            RuleFor(x => x.Input.TargetMeters).Must(FabricStock.IsValidMeters)
                .WithMessage("Фактичний залишок — число до 1 000 000 м, до сотих.");
            RuleFor(x => x.Input.OperationDate).OperationDate();
            RuleFor(x => x.Input.Reason).Must(x => x?.Trim().Length is >= ReasonMinLength and <= ReasonMaxLength)
                .WithMessage(StockMessages.Reason);
        });
    }
}
