using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.FabricNorms.GetNorms;

public sealed class GetFabricNormsQueryHandler(IProductionDbContext db, IProductionModelReader models)
    : IQueryHandler<GetFabricNormsQuery, Result<IReadOnlyList<FabricNormRowResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<FabricNormRowResponse>>> Handle(GetFabricNormsQuery query,
        CancellationToken cancellationToken)
    {
        var catalog = await models.ListSewingModelsAsync(cancellationToken);
        var norms = await db.FabricNorms.AsNoTracking().ToDictionaryAsync(x => x.ModelCode, cancellationToken);
        var rows = new List<FabricNormRowResponse>(catalog.Count + norms.Count);
        foreach (var model in catalog.OrderBy(x => x.Code, StringComparer.Ordinal))
        {
            norms.Remove(model.Code, out var norm);
            var values = norm?.Values;
            rows.Add(new(model.Code, model.Name, model.IsActive, values?.ToInput(), values?.TwoColorsTotal,
                values?.ThreeColorsTotal, norm?.UpdatedAt, null, model.ProductId, model.Season));
        }

        rows.AddRange(norms.Values.OrderBy(x => x.ModelCode, StringComparer.Ordinal).Select(x => new FabricNormRowResponse(
            x.ModelCode, null, false, x.Values.ToInput(), x.Values.TwoColorsTotal, x.Values.ThreeColorsTotal,
            x.UpdatedAt, FabricNormMessages.ModelNotFound)));
        return Result.Success<IReadOnlyList<FabricNormRowResponse>>(rows);
    }
}
