using Catalog.Application.Features.Pricing.SettingsSync;
using Catalog.Domain.Pricing.Ppe;
using Mediator;
using Reference.Application.Contracts.Catalog;
using Reference.Application.Features.PpeMarkupRule.GetList;
using Reference.Application.Features.PricingParameter.GetList;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>Hands the saved «Параметри ціноутворення» to Catalog sewing settings and PPE rule versions.</summary>
internal sealed class CatalogPricingSettingsSync(ISender sender, ILogger<CatalogPricingSettingsSync> logger)
    : IPricingSettingsChangeNotifier
{
    public const string Actor = "pricing-parameters";

    public async Task PricingSettingsChangedAsync(CancellationToken cancellationToken)
    {
        try
        {
            var parameters = await sender.Send(new GetPricingParametersQuery(), cancellationToken);
            var rules = await sender.Send(new GetPpeMarkupRulesQuery(), cancellationToken);
            if (!parameters.IsSuccess || !rules.IsSuccess)
            {
                logger.LogWarning("Pricing settings sync skipped: Reference lists could not be read.");
                return;
            }

            var result = await sender.Send(new SyncPricingSettingsCommand(
                parameters.Value.Where(x => x.Value is not null).ToDictionary(x => x.Code, x => x.Value!.Value),
                rules.Value.Select(x => new PpeMarkupRuleInput(x.Name,
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
