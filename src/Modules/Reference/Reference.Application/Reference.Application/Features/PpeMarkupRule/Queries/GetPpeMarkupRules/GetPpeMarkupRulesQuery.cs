
namespace Reference.Application.Features.PpeMarkupRule.Queries.GetPpeMarkupRules;

/// <summary>All PPE markup rules in sheet order; an empty list when there are none.</summary>
public sealed record GetPpeMarkupRulesQuery : IQuery<Result<List<PpeMarkupRuleRowDTO>>>;
