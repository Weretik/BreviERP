namespace Reference.Contracts.PricingParameters;

/// <summary>«Параметри ціноутворення» for other modules.</summary>
public interface IPricingParameterDirectory
{
    /// <summary>All rows, switched-off ones included.</summary>
    Task<IReadOnlyList<PricingParameterInfo>> ListAsync(CancellationToken cancellationToken);
}
