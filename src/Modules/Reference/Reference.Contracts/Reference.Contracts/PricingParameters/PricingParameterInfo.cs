namespace Reference.Contracts.PricingParameters;

/// <summary>A row of «Параметри ціноутворення» as other modules see it.</summary>
public sealed record PricingParameterInfo(
    int Id, string Code, string Name, decimal? Value, string Unit, DateOnly? UpdatedOn);
