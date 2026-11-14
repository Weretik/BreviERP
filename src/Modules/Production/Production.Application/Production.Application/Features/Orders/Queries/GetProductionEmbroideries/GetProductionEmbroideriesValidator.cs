using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Orders.Queries.GetProductionEmbroideries;

public sealed class GetProductionEmbroideriesValidator : AbstractValidator<GetProductionEmbroideriesQuery>
{
    public GetProductionEmbroideriesValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Search).MaximumLength(ItemApplication.TextMaxLength);
    }
}
