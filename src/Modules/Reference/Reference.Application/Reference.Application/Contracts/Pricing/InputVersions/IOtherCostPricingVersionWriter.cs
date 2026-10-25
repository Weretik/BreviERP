namespace Reference.Application.Contracts.Pricing;

public interface IOtherCostPricingVersionWriter
{
    Task<Result<long>> AppendAsync(string code, long expectedVersionId, decimal value, Guid requestId,
        CancellationToken cancellationToken);
}
