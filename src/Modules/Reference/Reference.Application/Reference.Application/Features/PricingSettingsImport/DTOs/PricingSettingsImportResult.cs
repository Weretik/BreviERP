using Reference.Application.Features.Shared.SheetImport;

namespace Reference.Application.Features.PricingSettingsImport.DTOs;

/// <summary>What happens (or happened) to one of the two tables.</summary>
/// <param name="Deleted">Brevi rows that are not in the sheet and are deleted.</param>
public sealed record PricingSettingsImportSection(
    int Created,
    int Updated,
    int Unchanged,
    int Errors,
    IReadOnlyList<string> Deleted,
    IReadOnlyList<ReferenceImportRowResult> Rows);

/// <summary>
/// Preview or outcome of importing «Налаштування цін». <paramref name="Applied"/> is false for a preview and
/// whenever any row has an error: then nothing is saved.
/// </summary>
public sealed record PricingSettingsImportResult(
    bool Applied,
    PricingSettingsImportSection Parameters,
    PricingSettingsImportSection PpeMarkupRules);
