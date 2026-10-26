using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Offers.CreateVersion;
using Catalog.Application.Features.Pricing.Ppe.Rules.CreateVersion;

namespace SewingPricing.Jobs.Activation;

public sealed class PpeInputConfirmation(IPpePricingInputReader reads, IPpeRuleVersionWriter rules, IPpeOfferVersionWriter offers)
{
    public async Task<Guid> RuleAsync(ActivationRulePlan plan, string actor, bool apply, CancellationToken cancellationToken)
    {
        var current = await reads.ReadRuleAsync(plan.Values.Code, cancellationToken);
        if (current is null || current.Values != plan.Values || current.Id != plan.ExpectedId && !current.IsApproved)
            throw new InvalidOperationException("PPE rule changed after planning.");
        if (current.IsApproved || !apply) return current.Id;
        var result = await new CreatePpeRuleVersionCommandHandler(rules)
            .Handle(new(plan.ExpectedId, plan.RequestId, plan.Values, actor), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException($"PPE rule confirmation: {result.Status}.");
        return result.Value.Id;
    }

    public async Task<Guid> OfferAsync(ActivationOfferPlan plan, string actor, bool apply, CancellationToken cancellationToken)
    {
        var input = (await reads.ReadAsync([plan.ProductId], cancellationToken)).SingleOrDefault();
        if (input is not { IsActive: true, Offer: { } current } || current.Values != plan.Values
            || current.Id != plan.ExpectedId && !current.IsApproved)
            throw new InvalidOperationException("PPE offer/activity changed after planning.");
        if (current.IsApproved || !apply) return current.Id;
        var result = await new CreatePpeOfferVersionCommandHandler(offers)
            .Handle(new(plan.ProductId, plan.ExpectedId, plan.RequestId, plan.Values, actor), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException($"PPE offer confirmation: {result.Status}.");
        return result.Value.Id;
    }
}
