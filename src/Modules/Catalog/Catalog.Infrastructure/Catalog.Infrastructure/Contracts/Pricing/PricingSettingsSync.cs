using Catalog.Application.Features.Pricing.SettingsSync;
using Catalog.Application.Features.Pricing.Sewing.AutoPublication;
using Catalog.Contracts.Pricing;
using Catalog.Domain.Pricing.Ppe;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reference.Contracts.PpeMarkupRules;
using Reference.Contracts.PricingParameters;

namespace Catalog.Infrastructure.Contracts.Pricing;

/// <summary>
/// Hands the saved «Параметри ціноутворення» to Catalog sewing settings and PPE rule versions, then publishes the new
/// Sewing prices (CAT-012). It runs from the outbox, outside the request, so it publishes itself; a publication
/// failure is only logged and the nightly brevierp-sync step publishes the prices.
/// </summary>
internal sealed class PricingSettingsSync(
    ISender sender,
    IPricingParameterDirectory parameterDirectory,
    IPpeMarkupRuleDirectory ruleDirectory,
    PricingInputChanges changes,
    IServiceScopeFactory scopes,
    ILogger<PricingSettingsSync> logger) : IPricingSettingsSync
{
    public const string Actor = "pricing-parameters";
    public const string PublicationActor = "auto";

    public async Task<string?> SettingsChangedAsync(CancellationToken cancellationToken)
    {
        var parameters = await parameterDirectory.ListAsync(cancellationToken);
        var rules = await ruleDirectory.ListAsync(cancellationToken);
        var result = await sender.Send(new SyncPricingSettingsCommand(
            parameters.Where(x => x.Value is not null).ToDictionary(x => x.Code, x => x.Value!.Value),
            rules.Select(x => new PpeMarkupRuleInput(x.Name,
                new PpeMarkups(x.RetailMarkup, x.WholesaleMarkup, x.LargeWholesaleMarkup))).ToList(),
            Actor), cancellationToken);
        if (!result.IsSuccess)
        {
            var reason = string.Join(" ", result.Errors.Concat(result.ValidationErrors.Select(x => x.ErrorMessage)));
            logger.LogWarning("Pricing settings sync failed: {Errors}", reason);
            return reason.Length > 0 ? reason : "Каталог не прийняв параметри ціноутворення.";
        }

        logger.LogInformation("Pricing settings synced: sewing {SewingUpdated}, PPE rules {PpeRules}.",
            result.Value.SewingUpdated, string.Join(", ", result.Value.PpeRulesUpdated));
        foreach (var reason in result.Value.Skipped)
            logger.LogWarning("Pricing settings sync skipped a part: {Reason}", reason);
        await PublishAsync();
        return null;
    }

    private async Task PublishAsync()
    {
        if (!changes.HasChanges) return;
        var productIds = changes.ProductIds?.ToList();
        changes.Clear();
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var report = await scope.ServiceProvider.GetRequiredService<SewingPricePublisher>()
                .PublishChangedAsync(productIds, PublicationActor, CancellationToken.None);
            logger.LogInformation("Sewing prices published after a pricing settings change: {Published} articles",
                report.Published.Count);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Sewing price publication after a pricing settings change failed; the nightly sync will retry.");
        }
    }
}
