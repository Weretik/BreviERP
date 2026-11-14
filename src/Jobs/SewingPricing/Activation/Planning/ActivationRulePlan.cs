using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;

namespace SewingPricing.Jobs.Activation;

public sealed record ActivationRulePlan(Guid ExpectedId, Guid RequestId, PpeRuleValues Values);
