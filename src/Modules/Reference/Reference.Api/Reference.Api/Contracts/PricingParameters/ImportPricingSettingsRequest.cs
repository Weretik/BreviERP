using Reference.Application.Features.PricingSettingsImport.DTOs;

namespace Reference.Api.Contracts.PricingParameters;

/// <summary>Both tables of the Calculator sheet «Налаштування цін»; dryRun returns the preview without saving.</summary>
public sealed record ImportPricingSettingsRequest(
    IReadOnlyList<PricingParameterImportRow> Parameters,
    IReadOnlyList<PpeMarkupRuleImportRow> PpeMarkupRules,
    bool DryRun = true,
    bool DeleteMissing = false);
