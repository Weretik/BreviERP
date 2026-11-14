using Production.Application.Abstractions;
using Production.Domain.Forecast;
using Production.Domain.Orders;
using Production.Application.Features.Forecast.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.PayrollParameters.Shared;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Forecast.Queries.GetForecast;

public sealed class GetForecastValidator : AbstractValidator<GetForecastQuery>
{
    public GetForecastValidator() => RuleFor(x => x.Actor).ValidActor();
}
