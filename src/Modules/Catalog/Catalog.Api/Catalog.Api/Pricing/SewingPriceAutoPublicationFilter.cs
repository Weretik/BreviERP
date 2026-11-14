using System.Security.Claims;
using Catalog.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.AutoPublication;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Pricing;

/// <summary>
/// Automatic Sewing price publication (CAT-012). After an action has saved a pricing input (Reference fabric,
/// accessory or operation, a model's composition, the sewing settings), publishes the new prices of the affected
/// models. A failure is only logged: the saved change stays and the nightly brevierp-sync step publishes it.
/// </summary>
internal sealed class SewingPriceAutoPublicationFilter(
    PricingInputChanges changes,
    IServiceScopeFactory scopes,
    ILogger<SewingPriceAutoPublicationFilter> logger) : IAsyncActionFilter
{
    public const string SystemActor = "auto";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await next();
        if (!changes.HasChanges)
            return;

        var productIds = changes.ProductIds?.ToList();
        changes.Clear();
        var actor = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            // A fresh scope: the action's DbContexts may still hold changes that failed to save.
            await using var scope = scopes.CreateAsyncScope();
            var report = await scope.ServiceProvider.GetRequiredService<SewingPricePublisher>()
                .PublishChangedAsync(productIds, string.IsNullOrWhiteSpace(actor) ? SystemActor : actor, CancellationToken.None);
            logger.LogInformation(
                "Sewing prices published automatically: {Published} articles, {Unchanged} unchanged, {Blocked} not calculable, skipped models: {Skipped}",
                report.Published.Count, report.Unchanged, report.Blocked.Count,
                report.Skipped.Count == 0 ? "none" : string.Join(", ", report.Skipped));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Automatic Sewing price publication failed; the nightly sync will retry.");
        }
    }
}
