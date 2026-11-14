using System.Security.Cryptography;
using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

/// <summary>
/// «Перенесення з v133» (WH-006): replaces the whole «Склад тканин» with the stock of the old «Цех» from its D1 export.
/// A dry run does all of it in a transaction and rolls back, so it checks exactly what an apply would write. Records
/// entered in Brevi itself are removed only with <paramref name="ConfirmReplace"/>. The same export again changes nothing.
/// </summary>
public sealed record TransferStockCommand(WarehouseActor Actor, StockTransferInput Input, bool DryRun,
    bool ConfirmReplace) : ICommand<Result<StockTransferResult>>;
