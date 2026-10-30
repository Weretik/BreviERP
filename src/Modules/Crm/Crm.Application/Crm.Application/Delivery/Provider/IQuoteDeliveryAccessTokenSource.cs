namespace Crm.Application.Delivery.Provider;

/// <summary>Host supplies a delegated token with gmail.send and gmail.readonly. Never log its value.</summary>
public interface IQuoteDeliveryAccessTokenSource
{
    ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken);
}
