using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Orders.Queries.GetProductionQuoteSources;

public sealed class GetProductionQuoteSourcesValidator : AbstractValidator<GetProductionQuoteSourcesQuery>
{
    public GetProductionQuoteSourcesValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Search).MaximumLength(200);
    }
}
