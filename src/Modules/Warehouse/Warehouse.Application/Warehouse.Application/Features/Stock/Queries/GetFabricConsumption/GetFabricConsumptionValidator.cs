using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricConsumption;

public sealed class GetFabricConsumptionValidator : AbstractValidator<GetFabricConsumptionQuery>
{
    public GetFabricConsumptionValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("Кінець періоду раніше за початок.");
        RuleFor(x => x).Must(x => x.To.DayNumber - x.From.DayNumber <= 3660).WithMessage("Період до 10 років.");
    }
}
