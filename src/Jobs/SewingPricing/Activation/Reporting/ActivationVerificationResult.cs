namespace SewingPricing.Jobs.Activation;

public sealed record ActivationVerificationResult(Guid PlanId, int PublishedSewingArticles, int CalculablePpeProducts);
