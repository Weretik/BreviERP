using System.Text.Json;
using Identity.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SewingPricing.Jobs.Activation;
using SewingPricing.Jobs.Review;

namespace Host.Seed.Jobs;

internal static class PricingActivationCommandRunner
{
    public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
    {
        string Required(string key) => configuration[key] ?? throw new InvalidOperationException($"Activation requires --{key}.");
        var connection = configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Missing local connection.");
        ActivationTargetGuard.Validate(connection);
        var path = Required("plan");
        await using var scope = services.CreateAsyncScope();
        var actor = await ActivationActorReader.RequireManagerAsync(scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(), Required("actor-id"));
        var options = new JsonSerializerOptions { WriteIndented = true };
        if (configuration["pricing-activate"] == "plan")
        {
            if (File.Exists(path)) throw new InvalidOperationException("Keep the existing immutable plan; do not overwrite request IDs.");
            var review = await PricingInputReviewJob.RunAsync(connection, Required("settings-source"), Required("ppe-source"));
            if (review.DifferentSubjects != 0 || review.Subjects.Any(x => x.HoldReasons.Count > 0 && x.Key is not ("724" or "931")))
                throw new InvalidOperationException("Source review requires an explicit next data decision.");
            var plan = await PricingActivationPlanner.BuildAsync(services, connection, actor, ["724", "931"]);
            if (plan.CatalogSourceHash != review.CatalogHash) throw new InvalidOperationException("Source changed during planning.");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(plan, options));
            Console.WriteLine($"Activation plan saved: {plan.Sewing.Sum(x => x.Articles.Count)} Sewing; {plan.Offers.Count} PPE; exclusions 724/931. No data changed.");
            return;
        }
        if (configuration["pricing-activate"] == "verify")
        {
            await ActivationFileGuard.CheckHashAsync(path, Required("plan-sha256"));
            var plan = JsonSerializer.Deserialize<PricingActivationPlan>(await File.ReadAllTextAsync(path))
                ?? throw new InvalidOperationException("Invalid activation plan.");
            ActivationTargetGuard.Validate(connection, plan.Database);
            if (plan.Actor != actor) throw new InvalidOperationException("Actor differs from plan.");
            var verified = await PricingActivationVerification.VerifyAsync(services, plan);
            await File.WriteAllTextAsync(Required("report"), JsonSerializer.Serialize(verified, options));
            Console.WriteLine($"Reopened and verified: {verified.PublishedSewingArticles} Sewing; {verified.CalculablePpeProducts} PPE.");
            return;
        }
        if (configuration["pricing-activate"] != "apply" || configuration["confirmed"] != "true")
            throw new InvalidOperationException("Select plan or explicitly confirmed apply.");
        var reportPath = Required("report");
        await ActivationFileGuard.CheckHashAsync(path, Required("plan-sha256"));
        await ActivationFileGuard.CheckBackupAsync(Required("backup"), Required("backup-sha256"));
        var approved = JsonSerializer.Deserialize<PricingActivationPlan>(await File.ReadAllTextAsync(path))
            ?? throw new InvalidOperationException("Invalid activation plan.");
        if (approved.Actor != actor || !approved.ExcludedSkus.ToHashSet(StringComparer.Ordinal).SetEquals(["724", "931"]))
            throw new InvalidOperationException("Actor or authorized exclusion scope differs from the plan.");
        async Task Persist(PricingActivationReport report)
        {
            var temporary = reportPath + ".tmp";
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(report, options));
            File.Move(temporary, reportPath, true);
        }
        var result = await PricingActivationJob.RunAsync(services, connection, approved, Persist);
        Console.WriteLine($"Local activation {result.Status}; {result.Steps.Count} recorded steps; exclusions preserved.");
    }
}
