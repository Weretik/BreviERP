namespace Production.Application.Features.FabricNorms.Shared;

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
