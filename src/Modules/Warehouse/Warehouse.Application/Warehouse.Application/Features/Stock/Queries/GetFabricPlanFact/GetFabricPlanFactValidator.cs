using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;

public sealed class GetFabricPlanFactValidator : AbstractValidator<GetFabricPlanFactQuery>
{
    public const int FilterMaxLength = 200;

    public GetFabricPlanFactValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Model).MaximumLength(FilterMaxLength);
        RuleFor(x => x.Client).MaximumLength(FilterMaxLength);
        RuleFor(x => x.Fabric).MaximumLength(FilterMaxLength);
        RuleFor(x => x).Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("Кінець періоду раніше за початок.");
    }
}
