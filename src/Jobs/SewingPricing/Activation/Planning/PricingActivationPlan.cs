namespace SewingPricing.Jobs.Activation;

public sealed record PricingActivationPlan(Guid Id, string Database, string Actor, DateTimeOffset CreatedAt,
    string CatalogSourceHash, IReadOnlyList<ActivationModelPlan> Sewing,
    IReadOnlyList<ActivationRulePlan> Rules, IReadOnlyList<ActivationOfferPlan> Offers,
    IReadOnlyList<string> ExcludedSkus);
