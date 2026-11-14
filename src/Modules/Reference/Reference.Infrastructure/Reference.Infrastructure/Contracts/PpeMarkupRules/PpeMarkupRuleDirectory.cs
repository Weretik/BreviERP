using Mediator;
using Reference.Contracts.PpeMarkupRules;
using Reference.Application.Features.PpeMarkupRule.Queries.GetPpeMarkupRules;

namespace Reference.Infrastructure.Contracts.PpeMarkupRules;

internal sealed class PpeMarkupRuleDirectory(ISender sender) : IPpeMarkupRuleDirectory
{
    public async Task<IReadOnlyList<PpeMarkupRuleInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPpeMarkupRulesQuery(), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Reference PPE markup rules could not be read.");
        return result.Value.Select(x => new PpeMarkupRuleInfo(x.Id, x.Name, x.RetailMarkup, x.WholesaleMarkup, x.LargeWholesaleMarkup)).ToArray();
    }
}
