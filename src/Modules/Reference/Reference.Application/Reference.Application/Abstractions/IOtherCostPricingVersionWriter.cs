namespace Reference.Application.Abstractions;

public interface IOtherCostPricingVersionWriter
{
    Task<Result<long>> AppendAsync(string code, long expectedVersionId, decimal value, Guid requestId,
        CancellationToken cancellationToken);
}
