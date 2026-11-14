using Catalog.Application.Features.Pricing.SettingsSync;
using Catalog.Contracts.Pricing;
using Catalog.Domain.Pricing.Ppe;
using Mediator;
using Microsoft.Extensions.Logging;
using Reference.Contracts.PpeMarkupRules;
using Reference.Contracts.PricingParameters;

namespace Catalog.Infrastructure.Contracts.Pricing;

/// <summary>Hands the saved «Параметри ціноутворення» to Catalog sewing settings and PPE rule versions.</summary>
internal sealed class PricingSettingsSync(
    ISender sender,
    IPricingParameterDirectory parameterDirectory,
    IPpeMarkupRuleDirectory ruleDirectory,
    ILogger<PricingSettingsSync> logger) : IPricingSettingsSync
{
    public const string Actor = "pricing-parameters";

    public async Task SettingsChangedAsync(CancellationToken cancellationToken)
    {
        try
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
                logger.LogWarning("Pricing settings sync failed: {Errors}", string.Join(" ", result.Errors));
                return;
            }

            logger.LogInformation("Pricing settings synced: sewing {SewingUpdated}, PPE rules {PpeRules}.",
                result.Value.SewingUpdated, string.Join(", ", result.Value.PpeRulesUpdated));
            foreach (var reason in result.Value.Skipped)
                logger.LogWarning("Pricing settings sync skipped a part: {Reason}", reason);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Pricing settings sync failed.");
        }
    }
}
