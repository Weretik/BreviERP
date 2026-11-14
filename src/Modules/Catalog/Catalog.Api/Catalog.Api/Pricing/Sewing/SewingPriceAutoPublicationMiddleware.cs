using System.Security.Claims;
using Catalog.Contracts.Pricing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Catalog.Application.Features.Pricing.Shared.Sewing.AutoPublication;

namespace Catalog.Api.Pricing.Sewing;

/// <summary>
/// Automatic Sewing price publication (CAT-012). After a request has saved a pricing input (Reference fabric,
/// accessory or operation, a model's composition, the sewing settings), publishes the new prices of the affected
/// models. A failure is only logged: the saved change stays and the nightly brevierp-sync step publishes it.
/// It is middleware, not an MVC filter, so it runs after Minimal API endpoints of every module as well.
/// </summary>
internal sealed class SewingPriceAutoPublicationMiddleware(
    RequestDelegate next,
    IServiceScopeFactory scopes,
    ILogger<SewingPriceAutoPublicationMiddleware> logger)
{
    public const string SystemActor = "auto";

    public async Task InvokeAsync(HttpContext context, PricingInputChanges changes)
    {
        await next(context);
        if (!changes.HasChanges)
            return;

        var productIds = changes.ProductIds?.ToList();
        changes.Clear();
        var actor = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            // A fresh scope: the request's DbContexts may still hold changes that failed to save.
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

/// <summary>Puts <see cref="SewingPriceAutoPublicationMiddleware"/> around the whole pipeline of the host.</summary>
internal sealed class SewingPriceAutoPublicationStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.UseMiddleware<SewingPriceAutoPublicationMiddleware>();
        next(app);
    };
}
