using Crm.Application.Delivery.Provider;

namespace Crm.Infrastructure.Delivery.Readiness;

/// <summary>Explicit non-network default until a reviewed delegated OAuth source is provisioned.</summary>
public sealed class UnavailableQuoteDeliveryAccessTokenSource : IQuoteDeliveryAccessTokenSource
{
    public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<string?>(null);
    }
}
