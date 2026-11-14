using Production.Application.Abstractions;
using Production.Domain;
using Production.Domain.Orders;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Commands.ImportProductionWork;

/// <summary>
/// Moves «Журнал робіт» of the old «Цех» onto orders already moved by «Імпорт» (PRD-D40). The item is found by the
/// order's legacy id and the position, the worker by full name. All or nothing; a repeat skips records already moved.
/// </summary>
public sealed record ImportProductionWorkCommand(ProductionActor Actor, IReadOnlyList<ProductionWorkImportRow> Rows,
    bool DryRun) : ICommand<Result<ProductionWorkImportResult>>;
