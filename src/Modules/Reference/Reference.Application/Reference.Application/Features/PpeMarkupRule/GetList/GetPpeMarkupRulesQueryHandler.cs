using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PpeMarkupRule.GetList.DTOs;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.Entities.PpeMarkupRule;

namespace Reference.Application.Features.PpeMarkupRule.GetList;

public sealed class GetPpeMarkupRulesQueryHandler(IReferenceReadRepository<PpeMarkupRuleEntity> repository)
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
