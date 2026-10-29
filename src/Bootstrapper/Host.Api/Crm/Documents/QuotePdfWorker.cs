using Crm.Application.Abstractions.Documents;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.Projection;
using Crm.Application.Documents.Source;

namespace Host.Api.Crm.Documents;

// Polls durable jobs; a lease token prevents an expired attempt from replacing a newer result.
public sealed class QuotePdfWorker(IServiceScopeFactory scopeFactory, ILogger<QuotePdfWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var artifacts = scope.ServiceProvider.GetRequiredService<IQuotePdfArtifactStore>();
                var claim = await artifacts.ClaimNextAsync(stoppingToken);
                if (claim is null) await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                else await RenderAsync(scope.ServiceProvider, artifacts, claim, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception error)
            {
                logger.LogError("Crm PDF worker loop failed: {ErrorType}", error.GetType().Name);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task RenderAsync(IServiceProvider services, IQuotePdfArtifactStore artifacts,
        QuotePdfArtifactClaim claim, CancellationToken cancellationToken)
    {
        try
        {
            var renderer = services.GetService<IPrivateDocumentRenderer>();
            if (renderer is null)
            {
                await artifacts.FailAsync(claim.Artifact.PublicId, claim.LeaseToken,
                    "renderer_unavailable", cancellationToken);
                return;
            }
            var source = await services.GetRequiredService<IQuoteDocumentSource>()
                .LoadRenderSourceAsync(claim.Artifact, cancellationToken);
            if (source is null)
            {
                await artifacts.FailAsync(claim.Artifact.PublicId, claim.LeaseToken,
                    "pinned_source_missing", cancellationToken);
                return;
            }
            var logo = source.Issuer.LogoAssetKey is { } key
                ? await services.GetRequiredService<IQuoteBrandAssetResolver>()
                    .ResolveAsync(key, cancellationToken) : null;
            var projection = QuotePublicDocumentProjectionBuilder.Build(source.Version,
                source.Issuer, source.Template, source.Issue.Number,
                source.PublicContactName, logo);
            var pdf = await renderer.RenderAsync(projection, cancellationToken);
            var completed = await artifacts.CompleteAsync(claim.Artifact.PublicId, claim.LeaseToken,
                pdf, cancellationToken);
            if (!completed.IsSuccess)
                logger.LogWarning("Crm PDF artifact completion was rejected for {ArtifactId}", claim.Artifact.PublicId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception error)
        {
            logger.LogWarning("Crm PDF artifact render failed for {ArtifactId}: {ErrorType}",
                claim.Artifact.PublicId, error.GetType().Name);
            await artifacts.FailAsync(claim.Artifact.PublicId, claim.LeaseToken,
                error is TimeoutException ? "render_timeout" : "render_failed", cancellationToken);
        }
    }
}
