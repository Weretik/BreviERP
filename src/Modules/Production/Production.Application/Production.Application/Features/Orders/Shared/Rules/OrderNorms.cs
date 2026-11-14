using Production.Application.Abstractions;
using Production.Domain.FabricNorms;

namespace Production.Application.Features.Orders.Shared.Rules;

internal static class OrderNorms
{
    /// <summary>Workshop norms of the given model codes, by code.</summary>
    public static async Task<IReadOnlyDictionary<string, FabricNormValues>> ReadAsync(IProductionDbContext db,
        IEnumerable<string?> modelCodes, CancellationToken cancellationToken)
    {
        var codes = modelCodes.OfType<string>().Distinct().ToArray();
        if (codes.Length == 0) return new Dictionary<string, FabricNormValues>();
        var norms = await db.FabricNorms.AsNoTracking().Where(x => codes.Contains(x.ModelCode))
            .ToListAsync(cancellationToken);
        return norms.ToDictionary(x => x.ModelCode, x => x.Values);
    }
}
