using Catalog.Application.Abstractions.Pricing;

namespace SewingPricing.Jobs.Activation;

public sealed record ActivationRulePlan(Guid ExpectedId, Guid RequestId, PpeRuleValues Values);
