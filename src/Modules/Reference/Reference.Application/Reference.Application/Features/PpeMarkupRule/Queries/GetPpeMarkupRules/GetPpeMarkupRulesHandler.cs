using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.PpeMarkupRule;
using Reference.Application.Abstractions;

namespace Reference.Application.Features.PpeMarkupRule.Queries.GetPpeMarkupRules;

public sealed class GetPpeMarkupRulesHandler(IReferenceReadRepository<PpeMarkupRuleEntity> repository)
    : IQueryHandler<GetPpeMarkupRulesQuery, Result<List<PpeMarkupRuleRowDTO>>>
{
    public async ValueTask<Result<List<PpeMarkupRuleRowDTO>>> Handle(
        GetPpeMarkupRulesQuery query, CancellationToken cancellationToken)
    {
        var rules = await repository.ListAsync(cancellationToken);

        return Result.Success(rules
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)
            .Select(x => new PpeMarkupRuleRowDTO(x.Id.Value, x.Name, x.RetailMarkup, x.WholesaleMarkup,
                x.LargeWholesaleMarkup, x.PriceNote, x.AppliesTo, x.SortOrder))
            .ToList());
    }
}
