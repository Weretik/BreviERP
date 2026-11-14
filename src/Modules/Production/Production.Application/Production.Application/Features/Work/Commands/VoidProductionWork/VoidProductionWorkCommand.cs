using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Commands.VoidProductionWork;

/// <summary>«Сторно» of a record with a reason (PRD-D36); the record stays in the log, crossed out.</summary>
public sealed record VoidProductionWorkCommand(ProductionActor Actor, int WorkId, VoidProductionWorkInput Input)
    : ICommand<Result<ProductionItemWorkResponse>>;
