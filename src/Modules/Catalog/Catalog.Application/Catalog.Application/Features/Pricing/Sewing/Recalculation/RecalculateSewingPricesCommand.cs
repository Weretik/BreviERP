using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.Recalculation;

/// <summary>
/// «Перерахувати всі» (CAT-011): saves and publishes a new calculation for every active Article whose live price is
/// calculable and was never published or differs from the published one. <paramref name="ProductIds"/> null means
/// every Sewing model. A dry run only reports.
/// </summary>
public sealed record RecalculateSewingPricesCommand(IReadOnlyList<int>? ProductIds, bool DryRun, string Actor)
    : ICommand<Result<SewingRecalculationReport>>;

public sealed record SewingRecalculationReport(
    bool Applied,
    int Unchanged,
    IReadOnlyList<SewingRecalculationChange> Changes,
    IReadOnlyList<SewingRecalculationBlocked> Blocked);

/// <summary>One Article whose price is (or would be) published; <paramref name="Before"/> is null when it had none.</summary>
public sealed record SewingRecalculationChange(
    int ProductId, string? ModelCode, long ArticleId, string SaleSku,
    SewingPriceTiers? Before, SewingPriceTiers After);

/// <summary>An active Article whose price cannot be calculated; its published price, if any, stays.</summary>
public sealed record SewingRecalculationBlocked(
    int ProductId, string? ModelCode, long ArticleId, string SaleSku, IReadOnlyList<string> Blockers);
