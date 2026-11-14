using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Queries.GetWorkChoices;

/// <summary>Workers who work and items of «Нове» orders, for «Додати роботу» in «Журнал робіт».</summary>
public sealed record GetWorkChoicesQuery(ProductionActor Actor) : IQuery<Result<WorkChoicesResponse>>;
