using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;

public sealed class GetWorkshopDashboardValidator : AbstractValidator<GetWorkshopDashboardQuery>
{
    public GetWorkshopDashboardValidator() => RuleFor(x => x.Actor).ValidActor();
}
