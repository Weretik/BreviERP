using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.ImportProductionProgress;

/// <summary>
/// Moves the етапи, file links and the Telegram mark of the old «Цех» onto orders already moved by «Імпорт».
/// <paramref name="DryRun"/> only checks. A repeat changes nothing: stages are set, not added, and a file link
/// already on the order is skipped.
/// </summary>
public sealed record ImportProductionProgressCommand(
    ProductionActor Actor, IReadOnlyList<ProductionProgressImportRow> Rows, bool DryRun)
    : ICommand<Result<ProductionProgressImportResult>>;
