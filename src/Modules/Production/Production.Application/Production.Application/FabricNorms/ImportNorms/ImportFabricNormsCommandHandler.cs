using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.FabricNorms;

namespace Production.Application.FabricNorms.ImportNorms;

public sealed class ImportFabricNormsCommandHandler(IProductionDbContext db, IProductionModelReader models, TimeProvider clock)
    : ICommandHandler<ImportFabricNormsCommand, Result<FabricNormImportResult>>
{
    public async ValueTask<Result<FabricNormImportResult>> Handle(ImportFabricNormsCommand command,
        CancellationToken cancellationToken)
    {
        var catalog = (await models.ListSewingModelsAsync(cancellationToken)).ToDictionary(x => x.Code);
        var existing = await db.FabricNorms.ToDictionaryAsync(x => x.ModelCode, cancellationToken);
        var now = clock.GetUtcNow();
        var lines = new List<FabricNormImportLine>(command.Rows.Count);
        var changes = new List<Action>();
        foreach (var row in command.Rows)
        {
            var code = ModelFabricNorm.NormalizeCode(row.ModelCode);
            var values = row.Norm.ToValues();
            if (!catalog.TryGetValue(code, out var model))
            {
                lines.Add(new(code, null, "error", FabricNormMessages.ModelNotFound));
                continue;
            }

            if (!existing.TryGetValue(code, out var norm))
            {
                changes.Add(() => db.FabricNorms.Add(new ModelFabricNorm(code, values, command.UserId, now)));
                lines.Add(new(code, model.Name, "created", null));
            }
            else if (norm.Values != values)
            {
                changes.Add(() => norm.Set(values, command.UserId, now));
                lines.Add(new(code, model.Name, "updated", null));
            }
            else lines.Add(new(code, model.Name, "unchanged", null));
        }

        var errors = lines.Count(x => x.Action == "error");
        if (!command.DryRun && errors == 0)
        {
            changes.ForEach(x => x());
            await db.SaveChangesAsync(cancellationToken);
        }

        // A list with a wrong row saves nothing; the lines say which rows to fix.
        return Result.Success(new FabricNormImportResult(command.DryRun, lines.Count(x => x.Action == "created"),
            lines.Count(x => x.Action == "updated"), lines.Count(x => x.Action == "unchanged"), errors, lines));
    }
}
