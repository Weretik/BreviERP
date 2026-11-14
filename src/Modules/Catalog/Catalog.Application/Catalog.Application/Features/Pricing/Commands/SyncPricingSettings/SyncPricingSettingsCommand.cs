using Catalog.Domain.Pricing.Ppe;

namespace Catalog.Application.Features.Pricing.Commands.SyncPricingSettings;

/// <summary>
/// The current «Параметри ціноутворення» from Reference: parameter values by sheet code and PPE markup rules by
/// name. Catalog appends a new approved sewing settings version and PPE rule versions only where a value changed,
/// so price history stays intact.
/// </summary>
public sealed record SyncPricingSettingsCommand(
    IReadOnlyDictionary<string, decimal> Parameters,
    IReadOnlyList<PpeMarkupRuleInput> PpeRules,
    string Actor) : ICommand<Result<PricingSettingsSyncResult>>;

public sealed record PpeMarkupRuleInput(string Name, PpeMarkups Markups);

/// <param name="SewingUpdated">A new sewing settings version was appended.</param>
/// <param name="PpeRulesUpdated">Names of PPE rules that got a new version.</param>
/// <param name="Skipped">Why a part was not synced, for logs.</param>
public sealed record PricingSettingsSyncResult(
    bool SewingUpdated,
    IReadOnlyList<string> PpeRulesUpdated,
    IReadOnlyList<string> Skipped);
