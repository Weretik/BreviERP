using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.Entities.PpeMarkupRule;

namespace Reference.Application.Features.PpeMarkupRule.Shared.Specifications;

public sealed class PpeMarkupRuleByNameSpec : Specification<PpeMarkupRuleEntity>
{
    public PpeMarkupRuleByNameSpec(string name) => Query.Where(x => x.Name == name);
}
