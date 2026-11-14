using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.ReceiveFabric;

/// <summary>
/// «Прихід»: the line of the exact offer picked grows (a new line when the pair never held that code). The supplier is
/// the name typed or picked, not a required counterparty.
/// </summary>
public sealed record ReceiveFabricCommand(WarehouseActor Actor, ReceiveFabricInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;
