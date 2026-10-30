using Crm.Application.Delivery.Processing;

namespace Host.Api.Crm.Delivery;

/// <summary>Each tick owns a scope; durable claim/finalization and external IO belong to the processor.</summary>
public sealed class QuoteDeliveryWorker(IServiceScopeFactory scopes, ILogger<QuoteDeliveryWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<QuoteDeliveryProcessor>();
                if (!await processor.ProcessOneAsync(stoppingToken))
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception error)
            {
                // Log only a bounded type name: provider bodies, addresses and secrets are private.
                logger.LogError("Crm delivery worker tick failed: {ErrorType}", error.GetType().Name);
                try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            }
        }
    }
}
