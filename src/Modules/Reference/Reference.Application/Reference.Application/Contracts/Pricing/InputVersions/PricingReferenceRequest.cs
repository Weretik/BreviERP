namespace Reference.Application.Contracts.Pricing;

public sealed record PricingReferenceRequest(
    IReadOnlyList<int> FabricIds, IReadOnlyList<int> AccessoryIds,
    IReadOnlyList<int> OperationIds, IReadOnlyList<string> OtherCostCodes);

