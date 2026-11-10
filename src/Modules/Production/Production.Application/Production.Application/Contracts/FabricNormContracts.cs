namespace Production.Application.Contracts;

/// <summary>Planned fabric per item in metres; see Production.Domain.FabricNorms.FabricNormValues.</summary>
public sealed record FabricNormInput(
    decimal? OneColor,
    decimal? TwoColorsMain,
    decimal? TwoColorsAdditional,
    decimal? ThreeColorsMain,
    decimal? ThreeColorsAdditional1,
    decimal? ThreeColorsAdditional2);

/// <summary>
/// A row of «Норми витрати тканини»: a Catalog sewing model with its norm, or a norm whose model is no longer in
/// Catalog. <paramref name="Problem"/> is shown only when something is wrong. <paramref name="ProductId"/> and
/// <paramref name="Season"/> come from Catalog; null when the model is gone or the season is not set.
/// </summary>
public sealed record FabricNormRowResponse(
    string ModelCode,
    string? ModelName,
    bool ModelIsActive,
    FabricNormInput? Norm,
    decimal? TwoColorsTotal,
    decimal? ThreeColorsTotal,
    DateTimeOffset? UpdatedAt,
    string? Problem,
    int? ProductId = null,
    string? Season = null);

public sealed record FabricNormImportRow(string ModelCode, FabricNormInput Norm);

public sealed record FabricNormImportResult(
    bool DryRun, int Created, int Updated, int Unchanged, int Errors, IReadOnlyList<FabricNormImportLine> Lines);

/// <param name="Action">created, updated, unchanged or error.</param>
public sealed record FabricNormImportLine(string ModelCode, string? ModelName, string Action, string? Error);
