using Reference.Application.Features.PricingSettingsImport.Shared;

namespace Reference.Application.Features.PricingSettingsImport.Commands.ImportPricingSettings;

/// <param name="DryRun">Only compute the preview; nothing is saved.</param>
/// <param name="DeleteMissing">Delete parameters and rules that are not in the sheet.</param>
public sealed record ImportPricingSettingsCommand(
    IReadOnlyList<PricingParameterImportRow> Parameters,
    IReadOnlyList<PpeMarkupRuleImportRow> PpeMarkupRules,
    bool DryRun,
    bool DeleteMissing) : ICommand<Result<PricingSettingsImportResult>>;
