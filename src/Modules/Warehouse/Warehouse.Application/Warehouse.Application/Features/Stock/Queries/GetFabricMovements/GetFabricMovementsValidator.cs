using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovements;

public sealed class GetFabricMovementsValidator : AbstractValidator<GetFabricMovementsQuery>
{
    public const int SearchMaxLength = 200;

    public GetFabricMovementsValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
        RuleFor(x => x.Kind).IsInEnum();
        RuleFor(x => x.FabricCode).MaximumLength(FabricStock.CodeMaxLength);
        RuleFor(x => x.Color).MaximumLength(StockColor.MaxLength);
        RuleFor(x => x.ItemId).GreaterThan(0).When(x => x.ItemId is not null);
        RuleFor(x => x.Search).MaximumLength(SearchMaxLength);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From!.Value)
            .When(x => x.From is not null && x.To is not null)
            .WithMessage("Кінець періоду не раніше початку.");
    }
}
