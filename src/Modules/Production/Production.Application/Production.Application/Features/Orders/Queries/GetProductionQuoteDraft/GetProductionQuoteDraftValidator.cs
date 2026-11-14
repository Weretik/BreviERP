using Production.Application.Features.FabricNorms.Shared;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Queries.GetProductionQuoteDraft;

public sealed class GetProductionQuoteDraftValidator : AbstractValidator<GetProductionQuoteDraftQuery>
{
    public GetProductionQuoteDraftValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.QuoteId).GreaterThan(0);
    }
}
