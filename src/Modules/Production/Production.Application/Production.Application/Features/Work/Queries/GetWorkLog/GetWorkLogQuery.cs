using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Queries.GetWorkLog;

/// <summary>«Журнал робіт» of a month «РРРР-ММ», optionally of one worker or one order.</summary>
public sealed record GetWorkLogQuery(ProductionActor Actor, string Month, int? EmployeeId, int? OrderId)
    : IQuery<Result<WorkLogResponse>>;
