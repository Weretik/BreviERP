using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.SettingsSync;

public sealed class SyncPricingSettingsCommandHandler(
    ISewingPricingSettingsReader sewingReader,
    ISewingPricingSettingsWriter sewingWriter,
    IPpePricingInputReader ppeReader,
    IPpeRuleVersionWriter ppeWriter,
    TimeProvider timeProvider)
    : ICommandHandler<SyncPricingSettingsCommand, Result<PricingSettingsSyncResult>>
{
    public const string Source = "Параметри ціноутворення";

    public async ValueTask<Result<PricingSettingsSyncResult>> Handle(
        SyncPricingSettingsCommand command, CancellationToken cancellationToken)
    {
        if (command.Parameters is null || command.PpeRules is null
            || string.IsNullOrWhiteSpace(command.Actor) || command.Actor.Length > 200)
            return Result.Invalid(new ValidationError("request", "Parameters, rules and actor are required."));

        var skipped = new List<string>();
        var sewingUpdated = await SyncSewingAsync(command, skipped, cancellationToken);
        var ppeUpdated = new List<string>();
        foreach (var rule in command.PpeRules)
        {
            if (await SyncPpeRuleAsync(rule, command.Actor, skipped, cancellationToken))
                ppeUpdated.Add(rule.Name);
        }

        return Result.Success(new PricingSettingsSyncResult(sewingUpdated, ppeUpdated, skipped));
    }

    private async Task<bool> SyncSewingAsync(
        SyncPricingSettingsCommand command, List<string> skipped, CancellationToken cancellationToken)
    {
        var (settings, missing) = SewingSettingsCodes.Map(command.Parameters);
        if (settings is null)
        {
            skipped.Add($"Sewing settings: missing {string.Join(", ", missing)}.");
            return false;
        }
        if (SewingSettingsValidation.Validate(settings) is { Count: > 0 } blockers)
        {
            skipped.Add($"Sewing settings: {string.Join(" ", blockers.Select(x => x.Message))}");
            return false;
        }

        var candidates = await sewingReader.ReadCandidatesAsync(timeProvider.GetUtcNow(), cancellationToken);
        if (candidates.Count == 0)
        {
            skipped.Add("Sewing settings: no base version to append to.");
            return false;
        }
        var current = candidates[0];
        if (current.Settings == settings)
            return false;

        var appended = await sewingWriter.AppendAsync(current.Id, Guid.NewGuid(), settings, command.Actor, cancellationToken);
        if (!appended.IsSuccess)
            skipped.Add($"Sewing settings: {string.Join(" ", appended.Errors)}");
        return appended.IsSuccess;
    }

    private async Task<bool> SyncPpeRuleAsync(
        PpeMarkupRuleInput rule, string actor, List<string> skipped, CancellationToken cancellationToken)
    {
        var current = await ppeReader.ReadRuleAsync(rule.Name, cancellationToken);
        if (current?.Values.Markups == rule.Markups)
            return false;

        var values = new PpeRuleValues(rule.Name, rule.Markups, Source,
            DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime).ToString("yyyy-MM-dd"));
        var appended = await ppeWriter.AppendAsync(current?.Id, Guid.NewGuid(), values, actor, cancellationToken);
        if (!appended.IsSuccess)
            skipped.Add($"PPE rule {rule.Name}: {string.Join(" ", appended.Errors)}");
        return appended.IsSuccess;
    }
}
