using Production.Application.Abstractions;
using Production.Domain.FabricNorms;

namespace Production.Application.Features.FabricNorms.Queries.GetModelCompositions;

public sealed class GetModelCompositionsHandler(IProductionModelReader models)
    : IQueryHandler<GetModelCompositionsQuery, Result<IReadOnlyList<ModelCompositionResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ModelCompositionResponse>>> Handle(GetModelCompositionsQuery query,
        CancellationToken cancellationToken)
    {
        var catalog = await models.ListSewingModelsAsync(cancellationToken);
        var compositions = (await models.ListCompositionsAsync(cancellationToken))
            .Where(x => x.ModelCode is not null)
            .GroupBy(x => x.ModelCode!, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);

        var rows = catalog
            .Where(model => !string.IsNullOrEmpty(model.Code))
            .OrderBy(model => model.Code, StringComparer.Ordinal)
            .Select(model =>
            {
                var lines = compositions.GetValueOrDefault(model.Code)?.Lines ?? [];
                var layers = lines
                    .Select(line => (Purpose: ModelLayer.PurposeOf(line.Name), Line: line))
                    .Where(x => x.Purpose is not null)
                    .Select(x => new ModelLayerResponse(x.Purpose!, x.Line.Code, x.Line.Name, x.Line.Quantity, x.Line.Unit))
                    .ToList();
                return new ModelCompositionResponse(model.Code, model.Name, model.ProductId, model.Season,
                    model.IsActive, layers,
                    lines.Select(line => new ModelCompositionLineResponse(line.Kind, line.Article, line.Code, line.Name,
                        line.Quantity, line.Unit, line.Comment)).ToList());
            })
            .ToList();
        return Result.Success<IReadOnlyList<ModelCompositionResponse>>(rows);
    }
}
