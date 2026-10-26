using Catalog.Application.Contracts.Pricing.Ppe;

namespace SewingPricing.Jobs.Activation;

public sealed record ActivationRulePlan(Guid ExpectedId, Guid RequestId, PpeRuleValues Values);
