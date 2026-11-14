using System.Security.Cryptography;
using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

public sealed class TransferStockValidator : AbstractValidator<TransferStockCommand>
{
    public const int MaxRows = 20_000;

    public TransferStockValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Snapshot).MaximumLength(StockTransfer.SnapshotMaxLength);
            RuleFor(x => x.Input.FabricStock).NotNull().Must(x => x.Count is > 0 and <= MaxRows)
                .WithMessage("У вигрузці немає таблиці fabric_stock.");
            RuleFor(x => x.Input.FabricStockMovements).NotNull().Must(x => x.Count is > 0 and <= MaxRows)
                .WithMessage("У вигрузці немає таблиці fabric_stock_movements.");
            RuleFor(x => x.Input.FabricMovementRevisions).NotNull().Must(x => x.Count <= MaxRows);
            RuleFor(x => x.Input.FabricIssueBatches).NotNull().Must(x => x.Count <= MaxRows);
            RuleFor(x => x.Input.FabricIssueLines).NotNull().Must(x => x.Count <= MaxRows);
            RuleFor(x => x.Input.FabricIssueComponents).NotNull().Must(x => x.Count <= MaxRows);
            RuleFor(x => x.Input.FabricReservations).NotNull().Must(x => x.Count <= MaxRows);
            RuleFor(x => x.Input.ProductionOrders).NotNull().Must(x => x.Count <= MaxRows);
            RuleFor(x => x.Input.ProductionItems).NotNull().Must(x => x.Count <= MaxRows);
        });
    }
}
