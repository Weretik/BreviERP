using Reference.Domain.PpeMarkupRules;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.PpeMarkupRule;

namespace Reference.Application.Features.PpeMarkupRule.Shared.Specifications;

public sealed class PpeMarkupRuleByIdSpec : Specification<PpeMarkupRuleEntity>
{
    public PpeMarkupRuleByIdSpec(int id) => Query.Where(x => x.Id == PpeMarkupRuleId.From(id));
}
