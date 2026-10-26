namespace SewingPricing.Jobs.Activation;

public sealed record PricingActivationStep(string Resource, string Action, string? Before, string? After,
    Guid? CalculationId = null, Guid? PublicationId = null);
