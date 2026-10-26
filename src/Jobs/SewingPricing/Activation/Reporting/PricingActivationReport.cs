namespace SewingPricing.Jobs.Activation;

public sealed class PricingActivationReport
{
    public Guid PlanId { get; init; }
    public string Actor { get; init; } = string.Empty;
    public string Status { get; set; } = "Running";
    public List<PricingActivationStep> Steps { get; } = [];
}
