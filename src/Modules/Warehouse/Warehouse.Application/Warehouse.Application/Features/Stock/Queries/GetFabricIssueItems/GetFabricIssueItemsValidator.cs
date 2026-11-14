using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricIssueItems;

public sealed class GetFabricIssueItemsValidator : AbstractValidator<GetFabricIssueItemsQuery>
{
    public GetFabricIssueItemsValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
