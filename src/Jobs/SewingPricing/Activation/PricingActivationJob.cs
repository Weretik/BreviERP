using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
using Microsoft.Extensions.DependencyInjection;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace SewingPricing.Jobs.Activation;

public static class PricingActivationJob
{
    public static async Task<PricingActivationReport> RunAsync(IServiceProvider services, string connectionString,
        PricingActivationPlan plan, Func<PricingActivationReport, Task> persist, CancellationToken cancellationToken = default)
    {
        ActivationTargetGuard.Validate(connectionString, plan.Database);
        if (string.IsNullOrWhiteSpace(plan.Actor) || plan.Id == Guid.Empty
            || plan.Offers.Any(x => plan.ExcludedSkus.Contains(x.SaleSku))
            || plan.Sewing.SelectMany(x => x.Articles).Any(x => plan.ExcludedSkus.Contains(x.SaleSku)))
            throw new InvalidOperationException("Invalid activation actor, plan or exclusion scope.");
        await using (var checkScope = services.CreateAsyncScope())
            ActivationTargetGuard.Validate(checkScope.ServiceProvider.GetRequiredService<CatalogDbContext>().Database.GetConnectionString()!, plan.Database);
        var report = new PricingActivationReport { PlanId = plan.Id, Actor = plan.Actor };
        await persist(report);
        try
        {
            foreach (var model in plan.Sewing)
            {
                await using var scope = services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<SewingCompositionConfirmation>().ValidateAsync(model, cancellationToken);
                foreach (var article in model.Articles)
                    await scope.ServiceProvider.GetRequiredService<SewingActivationPublisher>().ValidateInputsAsync(model.ProductId, article, cancellationToken);
            }
            foreach (var rule in plan.Rules)
            {
                await using var scope = services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<PpeInputConfirmation>().RuleAsync(rule, plan.Actor, false, cancellationToken);
            }
            foreach (var offer in plan.Offers)
            {
                await using var scope = services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<PpeInputConfirmation>().OfferAsync(offer, plan.Actor, false, cancellationToken);
            }
            foreach (var rule in plan.Rules)
            {
                await using var scope = services.CreateAsyncScope();
                var id = await scope.ServiceProvider.GetRequiredService<PpeInputConfirmation>().RuleAsync(rule, plan.Actor, true, cancellationToken);
                report.Steps.Add(new(rule.Values.Code, "RuleConfirmed", rule.ExpectedId.ToString(), id.ToString()));
                await persist(report);
            }
            foreach (var offer in plan.Offers)
            {
                await using var scope = services.CreateAsyncScope();
                var provider = scope.ServiceProvider;
                var id = await provider.GetRequiredService<PpeInputConfirmation>().OfferAsync(offer, plan.Actor, true, cancellationToken);
                var input = (await provider.GetRequiredService<IPpePricingInputReader>().ReadAsync([offer.ProductId], cancellationToken)).Single();
                if (PpeProductPricingEvaluator.Evaluate(input).Calculation.Prices != offer.ExpectedPrices)
                    throw new InvalidOperationException("Confirmed PPE result differs from planned backend result.");
                report.Steps.Add(new(offer.SaleSku, "OfferConfirmed", offer.ExpectedId.ToString(), id.ToString()));
                await persist(report);
            }
            foreach (var model in plan.Sewing)
            {
                await using var scope = services.CreateAsyncScope();
                var provider = scope.ServiceProvider;
                var version = await provider.GetRequiredService<SewingCompositionConfirmation>().ConfirmAsync(model, plan.Actor, cancellationToken);
                report.Steps.Add(new(model.ProductId.ToString(), "CompositionConfirmed", model.ExpectedVersion.ToString(), version.ToString()));
                await persist(report);
                foreach (var article in model.Articles)
                {
                    report.Steps.Add(await provider.GetRequiredService<SewingActivationPublisher>()
                        .PublishAsync(model.ProductId, version, article, plan.Actor, cancellationToken));
                    await persist(report);
                }
            }
            report.Status = "Complete";
            await persist(report);
            return report;
        }
        catch
        {
            report.Status = "PartialOrRejected";
            await persist(report);
            throw;
        }
    }
}
