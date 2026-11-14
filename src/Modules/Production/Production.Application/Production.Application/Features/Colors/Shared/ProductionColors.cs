using Production.Application.Abstractions;
using Production.Domain.FabricPlans;
using Production.Application.Abstractions.Materials;

namespace Production.Application.Features.Colors.Shared;

/// <summary>
/// The workshop picks colours only from «Кольори» (REF-006). A colour already saved on the record stays valid, so an
/// item or stock line made before the list keeps working.
/// </summary>
public sealed class ProductionColors(IProductionColorReader colors)
{
    public const string NotInList = "Колір «{0}» не знайдено в довіднику «Кольори». Оберіть колір зі списку.";

    public Task<IReadOnlyList<string>> ListAsync(CancellationToken cancellationToken) =>
        colors.ListActiveAsync(cancellationToken);

    /// <summary>An error for each colour that is neither active in «Кольори» nor in <paramref name="kept"/>.</summary>
    public async Task<IReadOnlyList<ValidationError>> CheckAsync(IEnumerable<string?> values, IEnumerable<string?> kept,
        CancellationToken cancellationToken)
    {
        var allowed = (await colors.ListActiveAsync(cancellationToken)).Select(PlanColor.Key)
            .Concat(kept.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => PlanColor.Key(x!)))
            .ToHashSet(StringComparer.Ordinal);
        return values.Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Where(x => !allowed.Contains(PlanColor.Key(x)))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Select(x => new ValidationError("Color", string.Format(NotInList, x)))
            .ToArray();
    }
}
