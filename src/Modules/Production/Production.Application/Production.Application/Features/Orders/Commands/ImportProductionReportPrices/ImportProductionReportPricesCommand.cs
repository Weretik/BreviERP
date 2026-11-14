using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Commands.ImportProductionReportPrices;

/// <summary>
/// «Ціни КП» for orders moved by «Імпорт»: the old «Цех» kept no КП price, so «Звіти цеху» could not value their
/// output. Only an item without a price takes one; <paramref name="DryRun"/> only checks, and a repeat changes nothing.
/// </summary>
public sealed record ImportProductionReportPricesCommand(
    ProductionActor Actor, IReadOnlyList<ProductionReportPriceImportRow> Rows, bool DryRun)
    : ICommand<Result<ProductionReportPriceImportResult>>;
