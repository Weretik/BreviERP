using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.CreateFabricIssue;

/// <summary>
/// «Видача» by actual cutting (TZ §7): products cut now and the real fabric, one or more pairs of the stock. The server
/// compares each planned part with P × n / N once, calls a line «планова» when it is the planned pair and «заміна»
/// otherwise, and spends each pair in order over its stock lines. One confirmed form is one package and one operation.
/// </summary>
public sealed record CreateFabricIssueCommand(WarehouseActor Actor, int OrderId, int ItemId, CreateFabricIssueInput Input)
    : ICommand<Result<ProductionItemFabricResponse>>;
